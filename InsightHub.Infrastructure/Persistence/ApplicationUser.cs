using InsightHub.Domain.Entities;
using InsightHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsightHub.Infrastructure.Persistence;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    [StringLength(50)]
    public string? Collage { get; set; }

    public bool IsEmployed { get; set; } = false;

    public int? TrackId { get; set; }
    public Track? Track { get; set; }

    public int? YearsExperience { get; set; }
    public bool HasCompletedAssessment { get; set; } = false;
    public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}
