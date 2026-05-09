using InsightHub.Application.Interfaces;
using InsightHub.Infrastructure.Persistence;
using InsightHub.Infrastructure.Services;
using InsightHub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace InsightHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICareerQuizService, CareerQuizService>();
        services.AddScoped<IInterviewQuizService, InterviewQuizService>();
        services.AddScoped<IJobsSyncService, JobsSyncService>();
        services.AddScoped<IJobOffersQuery, JobOffersQueryService>();
        services.AddScoped<INewsIngestionService, NewsIngestionService>();
        services.AddScoped<INewsQuery, NewsQueryService>();
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IUserSubmissionService, UserSubmissionService>();

        services.AddHttpClient<AdzunaService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<IAdzunaService>(sp => sp.GetRequiredService<AdzunaService>());

        services.AddHttpClient<NewsService>()
            .ConfigureHttpClient((_, client) =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "InsightHub/1.0");
            });
        services.AddScoped<INewsService>(sp => sp.GetRequiredService<NewsService>());

        services.AddHttpClient<InterviewQuestionsSyncService>();
        services.AddScoped<IInterviewQuestionsSyncService>(sp => sp.GetRequiredService<InterviewQuestionsSyncService>());

        return services;
    }
}