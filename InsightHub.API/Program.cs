using Hangfire;
using InsightHub.Application.Interfaces;
using InsightHub.Infrastructure;
using InsightHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace InsightHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllers();

            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("Jwt:Key is missing");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddRateLimiter(RateLimitPolicies.Register);

            builder.Services.AddMemoryCache();

            builder.Services.Configure<DataAnalysisSettings>(
                builder.Configuration.GetSection("DataAnalysis"));

            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "InsightHub API", Version = "v1" });
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token only"
                });

                options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", hostDocument: doc, externalResource: null),
                        new List<string>()
                    }
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                SeedData.InitializeAsync(db).GetAwaiter().GetResult();
                await DummyDataSeeder.SeedMarketDataAsync(db, userManager); //Add Dummy Users to Test the Assissmint
            }

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    var ex = error?.Error;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = ex?.Message,
                        inner = ex?.InnerException?.Message,
                        inner2 = ex?.InnerException?.InnerException?.Message
                    });
                });
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
         
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseHangfireDashboard();
            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

                recurringJobManager.AddOrUpdate<IJobsSyncService>(
                    "job-sync",
                    job => job.RunJobSyncPublic(),
                    Cron.Daily(1)
                );

                recurringJobManager.AddOrUpdate<INewsIngestionService>(
                    "news-ingest",
                    job => job.IngestAsync(),
                    Cron.HourInterval(12)
                );

                recurringJobManager.AddOrUpdate<IInterviewQuestionsSyncService>(
                    "interview-questions-sync",
                    job => job.FetchAndStoreQuestions(),
                    Cron.Weekly(DayOfWeek.Saturday, 0)
                );
            }

            app.Run();
        }
    }
}
