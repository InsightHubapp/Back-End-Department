using InsightHub.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsightHub.Application.ViewModels;

public class RegisterViewModel
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(50)]
    public string? Collage { get; set; }

    public bool IsEmployed { get; set; } = false;

    public int? TrackId { get; set; }
    public int? YearsExperience { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
