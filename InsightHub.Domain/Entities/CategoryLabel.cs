namespace InsightHub.Domain.Entities;

public class CategoryLabel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int TrackId { get; set; }
    public Track Track { get; set; } = null!;
}
