using System.ComponentModel.DataAnnotations;

namespace InsightHub.Application.ViewModels
{
    public class SubmitSurveyViewModel
    {
        [Required]
        public List<SurveyAnswerInput> Answers { get; set; } = new();
    }

    public class SurveyAnswerInput
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int AnswerValue { get; set; }
    }

    public class QuestionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? MaxValue { get; set; }
        public List<OptionViewModel> Options { get; set; } = new();
    }

    public class OptionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int NumericValue { get; set; }
    }

    public class MatchResultViewModel
    {
        public string MatchedUserName { get; set; } = string.Empty;
        public List<string> Jobs { get; set; } = new();
        public int TotalYearsExperience { get; set; }
        public double SimilarityScore { get; set; }
        public List<MatchedAnswerViewModel> EmployedAnswers { get; set; } = new();
    }

    public class MatchedAnswerViewModel
    {
        public string Question { get; set; } = string.Empty;
        public int Answer { get; set; }
        public string AnswerText { get; set; } = string.Empty; 
    }
}
