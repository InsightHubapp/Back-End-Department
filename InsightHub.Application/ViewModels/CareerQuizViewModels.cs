using System.ComponentModel.DataAnnotations;

namespace InsightHub.Application.ViewModels
{
    public class CareerQuizAnswerInput
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public bool Answer { get; set; } // Yes = true, No = false
    }

    public class SubmitCareerQuizViewModel
    {
        [Required]
        public List<CareerQuizAnswerInput> Answers { get; set; } = new();
    }

    public class TrackResultViewModel
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredSkills { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        //public object TrackSimilarityScore { get; set; }
    }

    public class CareerQuizResultViewModel
    {
        public List<TrackResultViewModel> TopTracks { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class FullMatchResultViewModel
    {
        public TrackResultViewModel Track { get; set; } = new();
        public MatchResultViewModel? BestMatch { get; set; }
        public MarketInsightsViewModel MarketInsights { get; set; } = new();
    }

    public class FullMatchAnswerInput
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int AnswerValue { get; set; }
    }

    public class SubmitFullMatchViewModel
    {
        [Required]
        public List<FullMatchAnswerInput> Answers { get; set; } = new();
    }

    public class UnifiedFullMatchResultViewModel
    {
        public List<TrackAverageMatchViewModel> TopTracks { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class TrackAverageMatchViewModel
    {
        public TrackResultViewModel Track { get; set; } = new();
        public double TrackSimilarityScore { get; set; }
        public string SimilarityMessage { get; set; } = string.Empty;
        public MarketInsightsViewModel MarketInsights { get; set; } = new();
    }

    public class MarketInsightsViewModel
    {
        public int TotalEmployeesInTrack { get; set; }
        public double AvgTechnicalLevel { get; set; }
        public double AvgSoftSkills { get; set; }
        public double AvgSalarysatisfaction { get; set; }
        public double AvgWorkLifeBalance { get; set; }
        public string MostCommonEnvironment { get; set; } = string.Empty;
        public string MostCommonCompanySize { get; set; } = string.Empty;
        public double AvgYearsExperience { get; set; }
        public double AvgConsistency { get; set; }
        public double AvgAdaptability { get; set; }
        public double AvgTeamwork { get; set; }
        public double AvgProblemSolving { get; set; }
        public double AvgLearningProactivity { get; set; }
        public double AvgCommunication { get; set; }
        public double AvgPrioritization { get; set; }
        public double AvgOwnership { get; set; }
        public double AvgCollaboration { get; set; }
        public double AvgResilience { get; set; }
    }
}

