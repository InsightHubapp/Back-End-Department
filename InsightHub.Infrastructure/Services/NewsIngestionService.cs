using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class NewsIngestionService : INewsIngestionService
{
    private readonly INewsService _newsService;
    private readonly AppDbContext _db;

    private static readonly string[] SpamKeywords =
    {
        "nfl", "nba", "soccer", "football", "baseball", "basketball",
        "celebrity", "divorce", "pregnant", "murder", "election",
        "politics", "trump", "biden", "congress", "bitcoin", "crypto",
        "ozempic", "weight loss", "recipe", "fashion", "travel",
        "horoscope", "weather", "stocks", "lawsuit", "arrest", "shooting",
        "shooter", "assassination", "killed", "dead",
        "dinner", "reunion", "husband", "wife", "marriage",
        "actor", "actress", "movie", "film", "music", "singer",
        "sports", "match", "goal", "player", "team", "league",
        "iran", "russia", "china", "government", "president",
        "hospital", "cancer", "medical", "health", "disease",
        "saree", "retail", "investment", "fund", "billionaire"
    };

    private static readonly Dictionary<string, (string[] Queries, string[] Keywords)> TrackConfig = new()
    {
        ["Backend Dev"] = (
        Queries: new[] { "backend API OR \"REST API\" OR \"ASP.NET\" OR \"Node.js\" OR microservice" },
        Keywords: new[] {
            "asp.net core", "node.js runtime", "express.js", "django rest", "fastapi",
            "laravel framework", "spring boot", "dotnet backend",
            "rest api design", "api gateway", "microservices architecture",
            "message queue", "rabbitmq", "grpc", "websocket server",
            "redis cache", "sql server", "postgresql", "mongodb atlas",
            "docker container", "kubernetes deployment", "nginx reverse proxy"
        }
    ),
        ["Frontend Dev"] = (
        Queries: new[] { "\"React.js\" OR \"Vue.js\" OR Angular OR frontend OR Tailwind" },
        Keywords: new[] {
            "react.js", "vue.js", "angular framework", "svelte", "tailwind css",
            "next.js app router", "nuxt.js",
            "webpack config", "vite bundler", "babel transpiler",
            "css animation", "responsive design", "dom manipulation",
            "component library", "ui framework", "browser rendering",
            "web accessibility", "html template", "css grid", "flexbox layout"
        }
    ),
        ["AI/ML"] = (
        Queries: new[] { "AI OR \"machine learning\" OR LLM OR ChatGPT OR \"artificial intelligence\"" },
        Keywords: new[] {
            "large language model", "openai gpt", "chatgpt", "anthropic claude",
            "google gemini", "deepseek", "mistral ai", "hugging face",
            "neural network", "model training", "fine-tuning", "inference engine",
            "transformer architecture", "generative ai", "natural language processing",
            "reinforcement learning", "diffusion model", "ai agent",
            "prompt engineering", "rag pipeline", "vector database"
        }
    ),
        ["Cybersecurity"] = (
        Queries: new[] { "cybersecurity OR \"data breach\" OR vulnerability OR ransomware OR hacking" },
        Keywords: new[] {
            "ransomware attack", "zero-day exploit", "sql injection", "xss vulnerability",
            "phishing campaign", "man in the middle", "ddos attack", "brute force attack",
            "penetration testing", "cve vulnerability", "cisa advisory",
            "malware analysis", "threat intelligence", "security patch",
            "firewall rules", "intrusion detection", "siem", "endpoint security",
            "data breach", "backdoor malware", "trojan horse", "encryption key"
        }
    ),
        ["Embedded"] = (
        Queries: new[] { "\"embedded systems\" OR IoT OR microcontroller OR Arduino OR \"Raspberry Pi\"" },
        Keywords: new[] {
            "arduino uno", "raspberry pi", "esp32", "stm32", "fpga design",
            "microcontroller firmware", "rtos scheduler",
            "embedded systems", "iot device", "hardware programming",
            "bare metal", "i2c protocol", "spi interface", "uart communication",
            "gpio pins", "interrupt handler", "bootloader", "circuit design",
            "sensor integration", "low power embedded"
        }
    ),
        ["Full Stack"] = (
        Queries: new[] { "\"full stack\" OR MERN OR \"web development\" OR \"full stack developer\"" },
        Keywords: new[] {
            "mern stack", "mean stack", "full stack developer", "full-stack web",
            "end to end web", "frontend and backend",
            "web application architecture", "ci cd pipeline", "devops workflow",
            "cloud deployment", "serverless function", "monorepo setup",
            "api integration frontend", "authentication flow", "session management"
        }
    ),
        ["Data Analysis"] = (
        Queries: new[] { "\"data science\" OR \"data analysis\" OR pandas OR \"Power BI\" OR Tableau" },
        Keywords: new[] {
            "pandas dataframe", "power bi dashboard", "tableau visualization",
            "jupyter notebook", "apache spark", "apache kafka", "dbt pipeline",
            "data science", "data engineering", "etl pipeline", "data warehouse",
            "machine learning model", "statistical analysis", "data cleaning",
            "feature engineering", "exploratory data analysis", "business intelligence",
            "data pipeline", "snowflake data", "bigquery"
        }
    ),
        ["Game Dev"] = (
        Queries: new[] { "\"game development\" OR \"game developer\" OR \"Unreal Engine\" OR gamedev OR Godot OR \"game engine\"" },
        Keywords: new[] {
            "unreal engine", "unity game engine", "godot engine", "game development",
            "indie game developer", "game studio",
            "shader programming", "game physics", "collision detection",
            "game loop", "sprite animation", "level design", "game mechanics",
            "multiplayer networking", "game optimization", "lod rendering",
            "procedural generation", "game ai", "pathfinding algorithm"
        }
    ),
        ["Mobile Dev"] = (
        Queries: new[] { "\"mobile development\" OR Flutter OR \"React Native\" OR \"Android development\" OR \"iOS development\"" },
        Keywords: new[] {
            "flutter widget", "react native app", "swift ios", "kotlin android",
            "jetpack compose", "xamarin forms", "swiftui",
            "mobile app development", "android studio", "xcode project",
            "app store submission", "google play store", "push notification",
            "mobile ui design", "offline storage mobile", "deep linking",
            "mobile performance", "apk build", "ios simulator"
        }
    ),
        ["Q/A Testing"] = (
        Queries: new[] { "\"software testing\" OR \"test automation\" OR Selenium OR Cypress OR \"QA engineer\"" },
        Keywords: new[] {
            "selenium webdriver", "cypress testing", "playwright automation",
            "jest unit test", "pytest framework", "jmeter load test",
            "test automation", "quality assurance", "tdd test driven",
            "bdd cucumber", "integration testing", "end to end testing",
            "regression testing", "test coverage", "qa engineer",
            "smoke testing", "performance testing", "bug tracking", "test suite"
        }
    ),
    };

    public NewsIngestionService(INewsService newsService, AppDbContext db)
    {
        _newsService = newsService;
        _db = db;
    }

    public async Task<int> IngestAsync()
    {
        var allArticles = new List<(NewsResponseVM Article, string Track)>();

        foreach (var (track, config) in TrackConfig)
        {
            foreach (var query in config.Queries)
            {
                for (int page = 1; page <= 3; page++)
                {
                    var result = await _newsService.GetRawNewsAsync(query, config.Keywords, page);
                    Console.WriteLine($"[{track}] page={page} '{query}' => {result.Count} articles");

                  if (result.Count == 0) break;

                    foreach (var article in result)
                        allArticles.Add((article, track));
                }
            }
        }

        var unique = allArticles
            .GroupBy(x => x.Article.Url)
            .Select(g => g.First())
            .ToList();

        Console.WriteLine($"Total unique: {unique.Count}");

        var filtered = unique
            .Where(x => IsValidArticle(x.Article, x.Track))
            .ToList();

        Console.WriteLine($"After strict filter: {filtered.Count}");

        var existingUrls = await _db.NewsArticles
            .Select(x => x.Url)
            .ToListAsync();

        int inserted = 0;
        foreach (var (article, track) in filtered)
        {
            var entity = NewsMapper.ToDomain(article, track);
            if (!existingUrls.Contains(entity.Url))
            {
                _db.NewsArticles.Add(entity);
                existingUrls.Add(entity.Url);
                inserted++;
            }
        }

        await _db.SaveChangesAsync();
        Console.WriteLine($"✅ News Ingestion Done. Inserted: {inserted} articles.");
        return inserted;
    }

    private bool IsValidArticle(NewsResponseVM a, string track)
    {
        if (string.IsNullOrEmpty(a.Title) ||
            string.IsNullOrEmpty(a.Description) ||
            string.IsNullOrEmpty(a.Content) ||
            string.IsNullOrEmpty(a.Url) ||
            string.IsNullOrEmpty(a.UrlToImage))
            return false;

        var text = (a.Title + " " + a.Description).ToLower();

        if (SpamKeywords.Any(k => text.Contains(k)))
            return false;

        var trackKeywords = TrackConfig[track].Keywords;
        if (!trackKeywords.Any(k => text.Contains(k)))
            return false;

        if (a.Title.Length < 20 || a.Title.Length > 200)
            return false;

        return true;
    }
}
