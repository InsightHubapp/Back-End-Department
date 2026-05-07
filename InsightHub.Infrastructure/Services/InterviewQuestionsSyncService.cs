using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace InsightHub.Infrastructure.Services;

public class InterviewQuestionsSyncService : IInterviewQuestionsSyncService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _config;

    private static readonly Dictionary<string, string[]> TrackTags = new()
    {
        ["Backend"] = new[]
     {
        "php", "laravel", "sql", "mysql", "postgresql",
        "docker", "kubernetes", "nginx", "redis",
        "rest-api", "graphql", "microservices", "sql-server",
        "nodejs", "express", "mongodb", "rabbitmq",
        "kafka", "grpc", "ci-cd", "aws", "azure", "mvc",
        "authentication", "authorization", "jwt", "oauth"
    },
        ["Frontend"] = new[]
     {
        "javascript", "typescript", "html", "css",
        "react", "vue", "angular", "webpack", "vite",
        "nextjs", "nuxt", "tailwind", "sass", "scss",
        "redux", "state-management", "pwa",
        "accessibility", "responsive-design", "web-performance",
        "browser", "dom", "seo"
    },
        ["AI/ML"] = new[]
     {
        "python", "machine-learning", "deep-learning",
        "tensorflow", "pytorch", "nlp", "pandas",
        "computer-vision", "transformers", "llm",
        "reinforcement-learning", "neural-networks",
        "data-preprocessing", "feature-engineering",
        "model-evaluation", "fine-tuning",
        "opencv", "scikit-learn", "huggingface"
    },
        ["Cybersecurity"] = new[]
     {
        "linux", "bash", "networking", "security",
        "cryptography", "devops",
        "penetration-testing", "ethical-hacking",
        "vulnerability-assessment", "firewalls",
        "owasp", "siem", "incident-response",
        "reverse-engineering", "malware-analysis",
        "web-security", "network-security", "ctf"
    },
        ["Embedded"] = new[]
     {
        "linux", "bash", "networking", "c", "cpp",
        "rtos", "microcontroller", "arduino", "raspberry-pi",
        "arm", "assembly", "uart", "spi", "i2c",
        "firmware", "bootloader", "memory-management",
        "embedded-linux", "yocto", "gpio", "hardware"
    },
        ["Data Analysis"] = new[]
     {
        "python", "sql", "mysql", "postgresql", "introduction to statistics",
        "pandas", "numpy", "matplotlib", "seaborn",
        "power-bi", "tableau", "excel",
        "data-cleaning", "data-visualization",
        "etl", "data-warehousing",
        "bigquery",
    },
        ["Game Dev"] = new[]
     {
        "unity", "unreal", "unreal-engine", "godot",
        "csharp", "cpp",
        "opengl", "directx", "vulkan",
        "physics", "pathfinding",
        "game-design", "game-mechanics",
        "shader", "graphics", "rendering",
        "multiplayer", "networking",
        "animation", "rigidbody", "collision-detection",
        "procedural-generation", "level-design",
        "audio", "vr", "ar"
    },
        ["Mobile"] = new[]
     {
        "javascript", "typescript", "react",
        "swift", "kotlin", "android",
        "react-native", "flutter", "dart",
        "ios", "xcode", "android-studio",
        "mobile-ui", "push-notifications",
        "offline-storage", "sqlite", "firebase",
        "app-performance", "play-store", "app-store"
    },
        ["Q/A Testing"] = new[]
     {
        "javascript", "typescript", "python", "sql",
        "selenium", "testing", "git",
        "cypress", "playwright", "jest",
        "unit-testing", "integration-testing", "e2e-testing",
        "test-automation", "api-testing", "postman",
        "performance-testing", "jmeter", "load-testing",
        "bug-reporting", "test-cases", "agile", "scrum"
    }
    };

    private static readonly (string Difficulty, int Pages)[] DifficultyConfig = new[]
    {
        ("Medium", 2),
        ("Hard", 1),
    };

    public InterviewQuestionsSyncService(
        HttpClient httpClient,
        AppDbContext dbContext,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _config = config;
    }

    public async Task<List<InterviewQuestionsVM>> GetQuestionsFromApi(string[] tags, string difficulty, int offset = 0)
    {
        var apiKey = _config["QuizAPI:api_key"]?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("QuizAPI key is missing.");

        var tagsParam = string.Join(",", tags);

        var url = $"https://quizapi.io/api/v1/questions?limit=50&offset={offset}&difficulty={difficulty}&tags={tagsParam}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"QuizAPI Error: {response.StatusCode}");
            Console.WriteLine(json);
            return new List<InterviewQuestionsVM>();
        }

        var result = JsonSerializer.Deserialize<QuizApiResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data ?? new List<InterviewQuestionsVM>();
    }

    public InterviewQuestion MapToEntity(InterviewQuestionsVM q, string track)
    {
        return new InterviewQuestion
        {
            ExternalId = q.Id,
            Question = q.Text,
            Type = q.Type ?? "unknown",
            Difficulty = q.Difficulty ?? "Medium",
            Explanation = q.Explanation,
            Category = track,
            Tags = q.Tags ?? new List<string>(),
            Answers = q.Answers?
                .Select((a, index) => new InterviewQoption
                {
                    Option = a.Option,
                    Number = index + 1,
                    IsCorrect = a.IsCorrect
                })
                .ToList() ?? new List<InterviewQoption>()
        };
    }

    public async Task FetchAndStoreQuestions()
    {
        var existingQuestions = await _dbContext.Set<InterviewQuestion>()
            .Select(q => new { q.ExternalId, q.Question })
            .ToListAsync();

        var existingExternalIds = new HashSet<string>(existingQuestions.Select(q => q.ExternalId));
        var existingTexts = new HashSet<string>(existingQuestions.Select(q => q.Question));

        int inserted = 0;

        foreach (var (track, tags) in TrackTags)
        {
            foreach (var (difficulty, pages) in DifficultyConfig)
            {
                for (int page = 0; page < pages; page++)
                {
                    int offset = page * 50;

                    var questions = await GetQuestionsFromApi(tags, difficulty, offset);

                    Console.WriteLine($"[{track}] {difficulty} offset={offset} => {questions.Count}");

                    if (questions == null || questions.Count == 0)
                        break;

                    foreach (var q in questions)
                    {
                        if (string.IsNullOrEmpty(q?.Id) || string.IsNullOrEmpty(q?.Text))
                            continue;
                        if (existingExternalIds.Contains(q.Id) || existingTexts.Contains(q.Text))
                            continue;

                        var entity = MapToEntity(q, track);
                        _dbContext.Set<InterviewQuestion>().Add(entity);

                        existingExternalIds.Add(q.Id);
                        existingTexts.Add(q.Text);
                        inserted++;
                    }
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"✅ Sync completed. Inserted: {inserted}");
    }
}
