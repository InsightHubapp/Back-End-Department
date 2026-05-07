namespace InsightHub.Domain.Entities;

public class QuizResult
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<QuizResultTrack> Tracks { get; set; } = new List<QuizResultTrack>();
}
