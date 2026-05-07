using System.ComponentModel.DataAnnotations;

namespace InsightHub.Application.ViewModels
{
    public class EmailExistance
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
