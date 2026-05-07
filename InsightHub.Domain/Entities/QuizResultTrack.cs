namespace InsightHub.Domain.Entities
{
    public class QuizResultTrack
    {
        public int Id { get; set; }
        public int QuizResultId { get; set; }
        public QuizResult QuizResult { get; set; } = null!;

        public int? TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequiredSkills { get; set; } = string.Empty;
        public string SimilarityMessage { get; set; } = string.Empty;
        public string MostCommonEnvironment { get; set; } = string.Empty;
        public string MostCommonCompanySize { get; set; } = string.Empty;

        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public double TrackSimilarityScore { get; set; }
        public int TotalEmployeesInTrack { get; set; }
        public double AvgTechnicalLevel { get; set; }
        public double AvgSoftSkills { get; set; }
        public double AvgSalarySatisfaction { get; set; }
        public double AvgWorkLifeBalance { get; set; }
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
