namespace InsightHub.Application.ViewModels;

public class InterviewQuestionsVM
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<InterviewAnswerVM> Answers { get; set; } = new();
}
