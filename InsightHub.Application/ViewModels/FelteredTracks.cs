using System.ComponentModel.DataAnnotations;

namespace InsightHub.Application.ViewModels
{
    public class FelteredTracks
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;
    }
}

