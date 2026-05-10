using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Services;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class CareerQuizService : ICareerQuizService
{
    private readonly AppDbContext _db;
    private static readonly int[] SharedQuestionIds = Enumerable.Range(111, 10).ToArray();
    private static readonly int[] MultiChoiceIds = { 113, 114, 115 };

    public CareerQuizService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CareerQuizQuestionViewModel>> GetQuestionsAsync()
    {
        var questions = await _db.Questions
            .AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.Id >= 111 && q.Id <= 160)
            .ToListAsync();

        return questions
            .OrderBy(_ => Random.Shared.Next())
            .Select(q => new CareerQuizQuestionViewModel
            {
                Id = q.Id,
                Text = q.Text,
                TrackId = q.TrackId,
                Type = (q.Id >= 113 && q.Id <= 115) ? "Choice" : q.Type.ToString(),
                Options = q.Options
                    .Select(o => new CareerQuizQuestionOptionViewModel
                    {
                        Id = o.Id,
                        Text = o.Text,
                        NumericValue = o.NumericValue
                    })
                    .ToList()
            })
            .ToList();
    }

    public async Task<CareerQuizStoredResultViewModel?> GetStoredResultAsync(string userId)
    {
        var result = await _db.QuizResults
            .AsNoTracking()
            .Include(r => r.Tracks)
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (result == null)
        {
            return null;
        }

        return new CareerQuizStoredResultViewModel
        {
            Id = result.Id,
            CreatedAt = result.CreatedAt,
            TopTracks = result.Tracks.Select(t => new CareerQuizStoredTrackViewModel
            {
                Track = new CareerQuizStoredTrackInfoViewModel
                {
                    TrackId = t.TrackId,
                    TrackName = t.TrackName,
                    Description = t.Description,
                    RequiredSkills = t.RequiredSkills,
                    Score = t.Score,
                    MaxScore = t.MaxScore,
                    Percentage = t.TrackSimilarityScore
                },
                TrackSimilarityScore = t.TrackSimilarityScore,
                SimilarityMessage = t.SimilarityMessage,
                MarketInsights = new CareerQuizStoredMarketInsightsViewModel
                {
                    TotalEmployeesInTrack = t.TotalEmployeesInTrack,
                    AvgTechnicalLevel = t.AvgTechnicalLevel,
                    AvgSoftSkills = t.AvgSoftSkills,
                    AvgSalarySatisfaction = t.AvgSalarySatisfaction,
                    AvgWorkLifeBalance = t.AvgWorkLifeBalance,
                    MostCommonEnvironment = t.MostCommonEnvironment,
                    MostCommonCompanySize = t.MostCommonCompanySize,
                    AvgYearsExperience = t.AvgYearsExperience,
                    AvgConsistency = t.AvgConsistency,
                    AvgAdaptability = t.AvgAdaptability,
                    AvgTeamwork = t.AvgTeamwork,
                    AvgProblemSolving = t.AvgProblemSolving,
                    AvgLearningProactivity = t.AvgLearningProactivity,
                    AvgCommunication = t.AvgCommunication,
                    AvgPrioritization = t.AvgPrioritization,
                    AvgOwnership = t.AvgOwnership,
                    AvgCollaboration = t.AvgCollaboration,
                    AvgResilience = t.AvgResilience
                }
            }).ToList()
        };
    }

    public async Task<UnifiedFullMatchResultViewModel> GetMatchResultAsync(string userId, SubmitFullMatchViewModel model)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException("Unauthorized user.");
        }

        if (user.IsEmployed)
        {
            throw new InvalidOperationException("This endpoint is for non-employed users only.");
        }

        if (model.Answers == null || model.Answers.Count == 0)
        {
            throw new InvalidOperationException("Answers are required.");
        }

        var submittedAnswerMap = model.Answers.ToDictionary(a => a.QuestionId, a => a.AnswerValue);
        var hasAllShared = CareerQuizDecisionEngine.HasAllSharedAnswers(submittedAnswerMap, SharedQuestionIds);
        if (!hasAllShared)
        {
            throw new InvalidOperationException("Please answer all career insight questions (111-120).");
        }

        var dbQuestions = await _db.Questions
            .AsNoTracking()
            .Include(q => q.Options)
            .Where(q => submittedAnswerMap.Keys.Contains(q.Id))
            .ToListAsync();

        var validAnswerMap = new Dictionary<int, int>();
        foreach (var question in dbQuestions)
        {
            var answerValue = submittedAnswerMap[question.Id];
            if (CareerQuizDecisionEngine.ValidateAnswer(question, answerValue) == null)
            {
                validAnswerMap[question.Id] = answerValue;
            }
        }

        await UpsertUserAnswersAsync(userId, validAnswerMap);

        var topTracks = await CalculateTopTracksAsync(validAnswerMap);
        var graduateSharedVector = CareerQuizDecisionEngine.BuildGraduateSharedVector(validAnswerMap, SharedQuestionIds);
        var allTrackResults = new List<TrackAverageMatchViewModel>();

        foreach (var track in topTracks)
        {
            var similarity = await CalculateTrackAverageSimilarityAsync(graduateSharedVector, track.TrackId);
            var marketInsights = await GetMarketInsightsAsync(track.TrackId);

            allTrackResults.Add(new TrackAverageMatchViewModel
            {
                Track = track,
                TrackSimilarityScore = similarity.Score,
                SimilarityMessage = similarity.Message,
                MarketInsights = marketInsights
            });
        }

        var topTrackResults = allTrackResults
            .OrderByDescending(t => t.TrackSimilarityScore)
            .ThenByDescending(t => t.Track.Percentage)
            .Take(3)
            .ToList();

        user.HasCompletedAssessment = true;
        await _db.SaveChangesAsync();
        await SaveQuizResultAsync(userId, topTrackResults);

        return new UnifiedFullMatchResultViewModel
        {
            TopTracks = topTrackResults,
            Message = topTrackResults.Count == 0 ? "No tracks matched." : $"Top recommendation: {topTrackResults[0].Track.TrackName}."
        };
    }

    private async Task SaveQuizResultAsync(string userId, List<TrackAverageMatchViewModel> topTracks)
    {
        var existing = await _db.QuizResults
            .Include(r => r.Tracks)
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (existing != null)
        {
            _db.QuizResults.Remove(existing);
        }

        var quizResult = new QuizResult
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Tracks = topTracks.Select(t => new QuizResultTrack
            {
                TrackId = t.Track.TrackId,
                TrackName = t.Track.TrackName ?? string.Empty,
                Description = t.Track.Description ?? string.Empty,
                RequiredSkills = t.Track.RequiredSkills ?? string.Empty,
                Score = t.Track.Score,
                MaxScore = t.Track.MaxScore,
                Percentage = t.TrackSimilarityScore,
                TrackSimilarityScore = t.TrackSimilarityScore,
                SimilarityMessage = t.SimilarityMessage ?? string.Empty,
                TotalEmployeesInTrack = t.MarketInsights?.TotalEmployeesInTrack ?? 0,
                AvgTechnicalLevel = t.MarketInsights?.AvgTechnicalLevel ?? 0,
                AvgSoftSkills = t.MarketInsights?.AvgSoftSkills ?? 0,
                AvgSalarySatisfaction = t.MarketInsights?.AvgSalarysatisfaction ?? 0,
                AvgWorkLifeBalance = t.MarketInsights?.AvgWorkLifeBalance ?? 0,
                MostCommonEnvironment = t.MarketInsights?.MostCommonEnvironment ?? "N/A",
                MostCommonCompanySize = t.MarketInsights?.MostCommonCompanySize ?? "N/A",
                AvgYearsExperience = t.MarketInsights?.AvgYearsExperience ?? 0,
                AvgConsistency = t.MarketInsights?.AvgConsistency ?? 0,
                AvgAdaptability = t.MarketInsights?.AvgAdaptability ?? 0,
                AvgTeamwork = t.MarketInsights?.AvgTeamwork ?? 0,
                AvgProblemSolving = t.MarketInsights?.AvgProblemSolving ?? 0,
                AvgLearningProactivity = t.MarketInsights?.AvgLearningProactivity ?? 0,
                AvgCommunication = t.MarketInsights?.AvgCommunication ?? 0,
                AvgPrioritization = t.MarketInsights?.AvgPrioritization ?? 0,
                AvgOwnership = t.MarketInsights?.AvgOwnership ?? 0,
                AvgCollaboration = t.MarketInsights?.AvgCollaboration ?? 0,
                AvgResilience = t.MarketInsights?.AvgResilience ?? 0
            }).ToList()
        };

        _db.QuizResults.Add(quizResult);
        await _db.SaveChangesAsync();
    }

    private async Task<(double Score, string Message)> CalculateTrackAverageSimilarityAsync(Dictionary<int, double> graduateAnswers, int trackId)
    {
        var employedIds = await _db.Users.AsNoTracking()
            .Where(u => u.IsEmployed && u.TrackId == trackId)
            .Select(u => u.Id)
            .ToListAsync();

        if (employedIds.Count == 0)
        {
            return (0, "No market data yet");
        }

        var responses = await _db.SurveyResponses.AsNoTracking()
            .Where(r => employedIds.Contains(r.UserId) && SharedQuestionIds.Contains(r.QuestionId))
            .ToListAsync();

        if (responses.Count == 0)
        {
            return (0, "No market data yet");
        }

        var score = CareerQuizDecisionEngine.ComputeSimilarityScore(
            SharedQuestionIds,
            MultiChoiceIds,
            graduateAnswers,
            responses);

        if (score <= 0)
        {
            return (0, "No market data yet");
        }

        return (score, string.Empty);
    }

    private async Task<MarketInsightsViewModel> GetMarketInsightsAsync(int trackId)
    {
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.IsEmployed && u.TrackId == trackId)
            .ToListAsync();

        if (users.Count == 0)
        {
            return new MarketInsightsViewModel { TotalEmployeesInTrack = 0 };
        }

        var userIds = users.Select(u => u.Id).ToList();
        var responses = await _db.SurveyResponses.AsNoTracking()
            .Where(r => userIds.Contains(r.UserId))
            .ToListAsync();

        var experienceValues = users
            .Where(u => u.YearsExperience.HasValue)
            .Select(u => (double)u.YearsExperience!.Value)
            .ToList();

        double GetAvg(int qId)
        {
            var vals = responses.Where(r => r.QuestionId == qId).Select(r => (double)r.AnswerValue).ToList();
            return vals.Count == 0 ? 0 : Math.Round(vals.Average(), 1);
        }

        int GetMode(int qId)
        {
            return responses
                .Where(r => r.QuestionId == qId)
                .GroupBy(r => r.AnswerValue)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()
                ?.Key ?? 0;
        }

        return new MarketInsightsViewModel
        {
            TotalEmployeesInTrack = users.Count,
            AvgYearsExperience = experienceValues.Count == 0 ? 0 : Math.Round(experienceValues.Average(), 1),
            AvgTechnicalLevel = GetAvg(116),
            AvgSoftSkills = GetAvg(117),
            AvgSalarysatisfaction = GetAvg(118),
            AvgWorkLifeBalance = GetAvg(119),
            MostCommonEnvironment = CareerQuizDecisionEngine.MapEnvironment(GetMode(113)),
            MostCommonCompanySize = CareerQuizDecisionEngine.MapCompanySize(GetMode(114)),
            AvgConsistency = GetAvg(101),
            AvgAdaptability = GetAvg(102),
            AvgTeamwork = GetAvg(103),
            AvgProblemSolving = GetAvg(104),
            AvgLearningProactivity = GetAvg(105),
            AvgCommunication = GetAvg(106),
            AvgPrioritization = GetAvg(107),
            AvgOwnership = GetAvg(108),
            AvgCollaboration = GetAvg(109),
            AvgResilience = GetAvg(110)
        };
    }

    private async Task<List<TrackResultViewModel>> CalculateTopTracksAsync(Dictionary<int, int> answerMap)
    {
        var quizQuestions = await _db.Questions.AsNoTracking()
            .Include(q => q.Track)
            .Where(q => q.IsCareerQuiz && q.TrackId != null)
            .ToListAsync();

        return quizQuestions
            .GroupBy(q => q.TrackId!.Value)
            .Select(g =>
            {
                var score = g.Count(q => answerMap.TryGetValue(q.Id, out var val) && val == 1);
                var max = g.Count();
                var track = g.First().Track;
                return new TrackResultViewModel
                {
                    TrackId = g.Key,
                    TrackName = track?.Name ?? "Unknown",
                    Description = track?.Description ?? string.Empty,
                    RequiredSkills = track?.RequiredSkills ?? string.Empty,
                    Score = score,
                    MaxScore = max,
                    Percentage = max == 0 ? 0 : Math.Round((double)score / max * 100, 1)
                };
            })
            .ToList(); 
    }
    private async Task UpsertUserAnswersAsync(string userId, Dictionary<int, int> answers)
    {
        var ids = answers.Keys.ToList();
        var existing = await _db.SurveyResponses
            .Where(r => r.UserId == userId && ids.Contains(r.QuestionId))
            .ToListAsync();

        foreach (var kvp in answers)
        {
            var match = existing.FirstOrDefault(r => r.QuestionId == kvp.Key);
            if (match != null)
            {
                match.AnswerValue = kvp.Value;
            }
            else
            {
                _db.SurveyResponses.Add(new SurveyResponse
                {
                    UserId = userId,
                    QuestionId = kvp.Key,
                    AnswerValue = kvp.Value
                });
            }
        }

        await _db.SaveChangesAsync();
    }

}
