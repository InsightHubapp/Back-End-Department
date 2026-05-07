using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class NewsQueryService : INewsQuery
{
    private readonly AppDbContext _context;

    public NewsQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArticalesVM>> GetArticlesAsync(
        IReadOnlyCollection<string> categories,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.NewsArticle> query = _context.NewsArticles.AsNoTracking();

        var normalized = categories
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalized.Count > 0 && !normalized.Contains("general"))
        {
            query = query.Where(a => normalized.Contains(a.Track));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var articles = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticalesVM
            {
                Title = a.Title,
                SourceName = a.SourceName,
                PublishedAt = a.PublishedAt,
                Url = a.Url,
                UrlToImage = a.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticalesVM>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = articles
        };
    }
}
