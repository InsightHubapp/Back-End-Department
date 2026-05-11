namespace InsightHub.Application.ViewModels;

public class CareerQuizQuestionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? TrackId { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<CareerQuizQuestionOptionViewModel> Options { get; set; } = new();
}

public class CareerQuizQuestionOptionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int NumericValue { get; set; }
}

public class CareerQuizStoredResultViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CareerQuizStoredTrackViewModel> TopTracks { get; set; } = new();
}

public class CareerQuizStoredTrackViewModel
{
    public CareerQuizStoredTrackInfoViewModel Track { get; set; } = new();
    public double TrackSimilarityScore { get; set; }
    public string SimilarityMessage { get; set; } = string.Empty;
    public double CombinedScore { get; set; }
    public CareerQuizStoredMarketInsightsViewModel MarketInsights { get; set; } = new();
}

public class CareerQuizStoredTrackInfoViewModel
{
    public int? TrackId { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequiredSkills { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public double Percentage { get; set; }
    public double CombinedScore { get; set; }
}

public class CareerQuizStoredMarketInsightsViewModel
{
    public int TotalEmployeesInTrack { get; set; }
    public double AvgTechnicalLevel { get; set; }
    public double AvgSoftSkills { get; set; }
    public double AvgSalarySatisfaction { get; set; }
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
