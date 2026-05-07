using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace InsightHub.Infrastructure.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public NewsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<NewsResponseVM>> GetRawNewsAsync(string query, string[]? keywords = null, int page = 1)
    {
        var apiKey = _config["NewsApi:ApiKey"];
        var from = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(0).ToString("yyyy-MM-dd");


        var finalQuery = query;
        if (keywords is { Length: > 0 })
        {
            var topKeywords = keywords.Take(8);
            var keywordQuery = string.Join(" OR ", topKeywords.Select(k => $"\"{k}\""));
            finalQuery = $"({query}) AND ({keywordQuery})";
        }

        var encoded = Uri.EscapeDataString(finalQuery);
        var url = $"https://newsapi.org/v2/everything?q={encoded}&language=en&pageSize=100&page={page}&sortBy=publishedAt&from={from}&to={to}&apiKey={apiKey}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "InsightHub/1.0");

        try
        {
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Body: {json.Substring(0, Math.Min(500, json.Length))}");
            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<NewsApiResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Articles ?? new List<NewsResponseVM>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<NewsResponseVM>();
        }
    }
}
