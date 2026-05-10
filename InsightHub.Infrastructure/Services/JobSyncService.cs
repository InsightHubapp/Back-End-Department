using Hangfire;
using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Services;
using InsightHub.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace InsightHub.Services
{
    public class JobsSyncService : IJobsSyncService
    {
        private readonly IServiceProvider _serviceProvider;

        public JobsSyncService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task RunJobSyncPublic()
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var adzunaService = scope.ServiceProvider.GetRequiredService<AdzunaService>();

            await RunJobSync(context, adzunaService);
        }

        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        private async Task RunJobSync(AppDbContext context, AdzunaService adzunaService)
        {
            var categoryMap = new Dictionary<string, List<string>>
            {
                ["Backend Dev"] = new() { "backend", "back-end", "node", "django", "flask", "spring", "laravel", "php", "asp.net", ".net", "golang", "rest api" },
                ["Frontend Dev"] = new() { "frontend", "react", "angular", "vue", "javascript", "typescript", "html", "css" },
                ["Full Stack"] = new() { "fullstack", "full-stack", "mern", "mean", "lamp" },
                ["Mobile Dev"] = new() { "mobile", "android", "ios", "flutter", "react native", "swift", "kotlin" },
                ["Game Dev"] = new() { "game", "unity", "unreal", "godot" },
                ["Q/A Testing"] = new() { "tester", "qa", "automation", "selenium", "cypress", "playwright", "sdet" },
                ["Embedded"] = new() { "embedded", "firmware", "iot", "microcontroller", "raspberry pi" },
                ["Data Analysis"] = new() { "data analyst", "data engineer", "bi", "etl", "sql", "power bi", "tableau" },
                ["AI/ML"] = new() { "ai", "machine learning", "ml", "nlp", "llm", "computer vision", "mlops" },
                ["Cybersecurity"] = new() { "cybersecurity", "security", "penetration", "soc", "ethical hacker", "devsecops" },
            };

            var searchKeywords = new List<string>
            {
                "backend developer","nodejs developer","django developer",
                "laravel developer","dotnet developer","php developer",
                "golang developer","spring boot developer",

                "frontend developer","react developer",
                "angular developer","vue developer","web developer",

                "fullstack developer","full stack developer",

                "mobile developer","android developer",
                "ios developer","flutter developer","react native developer",

                "game developer","unity developer","unreal developer",

                "qa engineer","automation tester","software tester","sdet",

                "embedded engineer","firmware engineer","iot developer",

                "data analyst","data engineer","data scientist","business analyst",

                "machine learning engineer","ai engineer","mlops engineer",

                "cybersecurity engineer","penetration tester","soc analyst"
            };

            var existingIds = await context.JobOffers
                .Where(x => x.ExternalId != null)
                .Select(x => x.ExternalId!)
                .ToHashSetAsync();

            foreach (var keyword in searchKeywords)
            {
                for (int page = 1; page <= 2; page++)
                {
                    var json = await adzunaService.GetJobs(keyword, page);

                    if (string.IsNullOrEmpty(json))
                        break;

                    var data = JsonConvert.DeserializeObject<AdzunaResponse>(json);

                    if (data?.results == null || !data.results.Any())
                        break;

                    var uniqueResults = data.results
                        .Where(x => x.id != null)
                        .GroupBy(x => x.id)
                        .Select(g => g.First());

                    foreach (var item in uniqueResults)
                    {
                        var externalId = item.id?.ToString();
                        if (string.IsNullOrEmpty(externalId)) continue;

                        if (existingIds.Contains(externalId) ||
                            context.JobOffers.Local.Any(x => x.ExternalId == externalId))
                            continue;

                        if (string.IsNullOrEmpty(item.title) ||
                            string.IsNullOrEmpty(item.company?.display_name) ||
                            string.IsNullOrEmpty(item.location?.display_name) ||
                            string.IsNullOrEmpty(item.description) ||
                            string.IsNullOrEmpty(item.redirect_url) ||
                            item.created == null)
                            continue;

                        var title = item.title.ToLower().Trim();

                        string? category = categoryMap
                            .FirstOrDefault(entry => entry.Value.Any(k => title.Contains(k)))
                            .Key;

                        if (category == null) continue;

                        existingIds.Add(externalId);

                        context.JobOffers.Add(new JobOffer
                        {
                            ExternalId = externalId,
                            Title = item.title,
                            CompanyName = item.company.display_name,
                            Location = item.location.display_name,
                            Description = item.description,
                            RedirectUrl = item.redirect_url,
                            Category = category,
                            CreatedDate = DateTime.Parse(item.created),
                            FetchedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            try
            {
                await context.SaveChangesAsync();
                Console.WriteLine("[JobsSync] Sync Done.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"[JobsSync] Duplicate error handled: {ex.Message}");
            }
            await context.Database.ExecuteSqlRawAsync(@"
                UPDATE JobOffers SET Category = 'Mobile' WHERE LOWER(Title) LIKE '%mobile%';

                UPDATE JobOffers SET Category = 'Full Stack'
                WHERE LOWER(Title) LIKE '%full stack%'
                   OR LOWER(Title) LIKE '%full-stack%'
                   OR LOWER(Title) LIKE '%fullstack%';

                UPDATE JobOffers SET Category = 'AI/ML'
                WHERE Category = 'Data Analysis'
                  AND (
                        LOWER(Title) LIKE '%ai%'
                     OR LOWER(Title) LIKE '%machine learning%'
                     OR LOWER(Title) LIKE '%mlops%'
                  );
            ");

            var cutoffDate = DateTime.UtcNow.AddMonths(-3);
            await context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM JobOffers WHERE CreatedDate < {0}
            ", cutoffDate);

            Console.WriteLine($"[JobsSync] Deleted old jobs older than {cutoffDate:yyyy-MM-dd}.");
        }
    }
}