namespace InsightHub.Application.Interfaces;

public interface INewsIngestionService
{
    Task<int> IngestAsync();
}
