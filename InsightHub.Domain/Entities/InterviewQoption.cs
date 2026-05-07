namespace InsightHub.Domain.Entities;

public class InterviewQoption
{
    public int Id { get; set; }
    public string Option { get; set; } = string.Empty;
    public int Number { get; set; }
    public bool IsCorrect { get; set; }
    public int InterviewQuestionId { get; set; }
    public virtual InterviewQuestion InterviewQuestion { get; set; } = null!;
}
