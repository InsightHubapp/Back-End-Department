namespace InsightHub.Infrastructure.Services;

public static class TrackKeywords
{
    public static Dictionary<string, string[]> Map = new()
    {
        ["Backend"] = new[] { "backend", "api", "server", "dotnet", "node" },
        ["Frontend"] = new[] { "frontend", "react", "ui", "css", "html" },
        ["AI"] = new[] { "ai", "machine learning", "llm" },
        ["Cybersecurity"] = new[] { "security", "hacking", "breach", "malware" },
        ["Fullstack"] = new[] { "fullstack", "web development" },
        ["Embedded Systems"] = new[] { "embedded", "iot", "microcontroller" },
        ["Testing"] = new[] { "testing", "qa", "automation" },
        ["Data Analysis"] = new[] { "data", "analytics", "sql", "pandas" },
        ["Mobile Development"] = new[] { "android", "ios", "mobile" },
        ["Game Development"] = new[] { "unity", "unreal", "game" }
    };
}
