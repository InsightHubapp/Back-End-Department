using InsightHub.Domain.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsightHub.Application.ViewModels;

public class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UserProfileViewModel
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Collage { get; set; } = string.Empty;
    public bool? IsEmployed { get; set; }
    public int? YearsExperience { get; set; }
    public string TrackName { get; set; } = string.Empty;
}
