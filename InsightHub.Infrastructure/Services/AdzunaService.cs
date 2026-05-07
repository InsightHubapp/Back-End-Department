using InsightHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InsightHub.Infrastructure.Services;

public class AdzunaService : IAdzunaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AdzunaService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GetJobs(string what, int page)
    {
        var appId = _config["Adzuna:AppId"];
        var appKey = _config["Adzuna:AppKey"];
        var url = $"https://api.adzuna.com/v1/api/jobs/gb/search/{page}?app_id={appId}&app_key={appKey}&what={what}&results_per_page=50";

        int maxRetries = 3;
        int delaySeconds = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (HttpRequestException ex) when (
                ex.Message.Contains("503") ||
                ex.Message.Contains("502") ||
                ex.Message.Contains("429"))
            {
                if (attempt == maxRetries)
                {
                    Console.WriteLine($"[AdzunaService] API failed after {maxRetries} attempts: {ex.Message}");
                    return string.Empty;
                }

                Console.WriteLine($"[AdzunaService] Attempt {attempt}/{maxRetries} failed. Retrying in {delaySeconds}s...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                delaySeconds *= 2;
            }
        }

        return string.Empty;
    }
}
