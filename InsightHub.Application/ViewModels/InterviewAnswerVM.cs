using System.Text.Json.Serialization;

namespace InsightHub.Application.ViewModels;

public class InterviewAnswerVM
{
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Option { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
