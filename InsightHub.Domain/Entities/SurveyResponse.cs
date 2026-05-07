using System.ComponentModel.DataAnnotations;

namespace InsightHub.Domain.Entities;

public class SurveyResponse
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int AnswerValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
