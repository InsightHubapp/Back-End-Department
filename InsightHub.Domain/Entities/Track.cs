using System.ComponentModel.DataAnnotations;

namespace InsightHub.Domain.Entities;

public class Track
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string RequiredSkills { get; set; } = string.Empty;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<CategoryLabel> CategoryLabels { get; set; } = new List<CategoryLabel>();
}
