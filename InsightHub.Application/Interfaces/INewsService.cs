using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface INewsService
{
    Task<List<NewsResponseVM>> GetRawNewsAsync(string query, string[]? keywords = null, int page = 1);
}
