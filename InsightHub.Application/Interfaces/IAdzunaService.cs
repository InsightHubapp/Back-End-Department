namespace InsightHub.Application.Interfaces;

public interface IAdzunaService
{
    Task<string> GetJobs(string what, int page);
}
