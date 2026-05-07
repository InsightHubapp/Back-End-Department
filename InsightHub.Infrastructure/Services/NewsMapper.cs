using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;

namespace InsightHub.Infrastructure.Services;

public static class NewsMapper
{
    public static NewsArticle ToDomain(NewsResponseVM dto, string track)
    {
        return new NewsArticle
        {
            Title = dto.Title,
            Url = dto.Url,
            ImageUrl = dto.UrlToImage,
            SourceName = dto.Source?.Name ?? string.Empty,
            PublishedAt = dto.PublishedAt,
            Track = track,
            Tags = ExtractTags(dto.Title + " " + dto.Description)
        };
    }

    private static List<string> ExtractTags(string text)
    {
        text = text.ToLower();

        var tags = new List<string>();
        if (text.Contains("api") || text.Contains("backend") || text.Contains("server") ||
            text.Contains("rest") || text.Contains("graphql") || text.Contains("microservice") ||
            text.Contains("node.js") || text.Contains("django") ||
            text.Contains("asp.net") || text.Contains("fastapi") || text.Contains("laravel"))
            tags.Add("Back-End");

        if (text.Contains("spring boot") || text.Contains("react") || text.Contains("frontend") || text.Contains("css") ||
            text.Contains("html") || text.Contains("vue") || text.Contains("angular") ||
            text.Contains("tailwind") || text.Contains("webpack") || text.Contains("vite") ||
            text.Contains("svelte") || text.Contains("typescript frontend"))
            tags.Add("Front-End");

        if (text.Contains("full stack") || text.Contains("fullstack") || text.Contains("next.js") ||
            text.Contains("mern stack") || text.Contains("mean stack") ||
            text.Contains("web development") || text.Contains("web app"))
            tags.Add("Full Stack");

        if (text.Contains("embedded") || text.Contains("iot") || text.Contains("microcontroller") ||
            text.Contains("raspberry pi") || text.Contains("arduino") || text.Contains("firmware") ||
            text.Contains("rtos") || text.Contains("stm32") || text.Contains("esp32") ||
            text.Contains("fpga") || text.Contains("embedded linux"))
            tags.Add("Embedded");

        if (text.Contains("machine learning") || text.Contains("deep learning") || text.Contains("neural network") ||
            text.Contains("llm") || text.Contains("generative ai") || text.Contains("nlp") ||
            text.Contains("gpt") || text.Contains("transformer") || text.Contains("fine-tuning") ||
            text.Contains("artificial intelligence") || text.Contains("chatgpt"))
            tags.Add("AI");

        if (text.Contains("analytics") || text.Contains("dataset") ||
            text.Contains("visualization") || text.Contains("pandas framework") || text.Contains("statistics") ||
            text.Contains("analysis") || text.Contains("business intelligence") || text.Contains("sql") ||
            text.Contains("data science") || text.Contains("power bi") || text.Contains("tableau") ||
            text.Contains("jupyter") || text.Contains("big data"))
            tags.Add("Data/Analysis");

        if (text.Contains("testing") || text.Contains("qa") || text.Contains("quality") ||
            text.Contains("selenium") || text.Contains("jest") || text.Contains("bug") ||
            text.Contains("debugging") || text.Contains("test") || text.Contains("automated") ||
            text.Contains("cypress") || text.Contains("playwright") || text.Contains("tdd"))
            tags.Add("QA/Testing");

        if (text.Contains("mobile") || text.Contains("android") || text.Contains("ios") ||
            text.Contains("flutter") || text.Contains("swift") || text.Contains("kotlin") ||
            text.Contains("react native") || text.Contains("smartphone") || text.Contains("app store") ||
            text.Contains("jetpack compose"))
            tags.Add("Mobile");

        if (text.Contains("game development") || text.Contains("game developer") ||
            text.Contains("unity engine") || text.Contains("unreal engine") ||
            text.Contains("godot") || text.Contains("gamedev") ||
            text.Contains("game engine") || text.Contains("indie game") ||
            text.Contains("game programming") || text.Contains("shader") ||
            text.Contains("pixel art") || text.Contains("multiplayer game") ||
            text.Contains("video game development") || text.Contains("game studio"))
            tags.Add("Game");

        if (text.Contains("cybersecurity") || text.Contains("vulnerability") || text.Contains("exploit") ||
            text.Contains("ransomware") || text.Contains("data breach") || text.Contains("malware") ||
            text.Contains("penetration testing") || text.Contains("zero-day") || text.Contains("phishing") ||
            text.Contains("ethical hacking") || text.Contains("cve") || text.Contains("hack") ||
            text.Contains("security"))
            tags.Add("Cybersecurity");

        return tags;
    }
}
