using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface IJobOffersQuery
{
    Task<PagedResult<FelteredJobs>> GetRelatedJobsAsync(IReadOnlyCollection<string> categories, int page, int pageSize, CancellationToken cancellationToken);
}
