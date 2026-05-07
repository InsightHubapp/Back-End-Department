using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface INewsQuery
{
    Task<PagedResult<ArticalesVM>> GetArticlesAsync(IReadOnlyCollection<string> categories, int page, int pageSize, CancellationToken cancellationToken);
}
