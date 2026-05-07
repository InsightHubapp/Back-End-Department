namespace InsightHub.Application.ViewModels;

public class InterviewQVM
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public List<InterviewQoptionVM> Answers { get; set; } = new();
}
