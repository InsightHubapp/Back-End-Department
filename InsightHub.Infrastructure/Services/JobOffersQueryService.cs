using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class JobOffersQueryService : IJobOffersQuery
{
    private readonly AppDbContext _context;

    public JobOffersQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FelteredJobs>> GetRelatedJobsAsync(
        IReadOnlyCollection<string> categories,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = categories
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query = _context.JobOffers.AsNoTracking();
        if (normalized.Count > 0 && !normalized.Contains("general"))
        {
            query = query.Where(x => x.Category != null && normalized.Contains(x.Category));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FelteredJobs
            {
                Title = x.Title,
                CompanyName = x.CompanyName,
                Location = x.Location,
                Description = x.Description,
                RedirectUrl = x.RedirectUrl,
                CreatedDate = x.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FelteredJobs>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = data
        };
    }
}