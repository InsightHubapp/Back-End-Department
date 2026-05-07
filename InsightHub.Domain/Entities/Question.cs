using InsightHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InsightHub.Domain.Entities;

public class Question
{
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public TargetGroup AppliesTo { get; set; }

    public int Order { get; set; }

    public int? MaxValue { get; set; }

    public bool IsCareerQuiz { get; set; } = false;

    public int? TrackId { get; set; }
    public Track? Track { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
}
