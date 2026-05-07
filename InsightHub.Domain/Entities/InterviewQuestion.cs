namespace InsightHub.Domain.Entities;

public class InterviewQuestion
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public virtual List<InterviewQoption> Answers { get; set; } = new();
}
