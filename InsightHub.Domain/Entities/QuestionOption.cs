using System.ComponentModel.DataAnnotations;

namespace InsightHub.Domain.Entities
{
    public class QuestionOption
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Text { get; set; } = string.Empty;

        // ????? ???? ?????? ?? vector ?? ??? matching
        public int NumericValue { get; set; }
    }
}
