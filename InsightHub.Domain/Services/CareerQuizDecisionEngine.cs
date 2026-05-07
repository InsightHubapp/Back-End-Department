using InsightHub.Domain.Entities;
using InsightHub.Domain.Enums;

namespace InsightHub.Domain.Services;

public static class CareerQuizDecisionEngine
{
    public static bool HasAllSharedAnswers(IReadOnlyDictionary<int, int> submittedAnswerMap, IReadOnlyCollection<int> sharedQuestionIds)
    {
        return sharedQuestionIds.All(submittedAnswerMap.ContainsKey);
    }

    public static string? ValidateAnswer(Question question, int value)
    {
        if (question.Type == QuestionType.YesNo && value is not (0 or 1))
        {
            return "Invalid YesNo";
        }

        if (question.Type == QuestionType.Scale && (value < 1 || value > (question.MaxValue ?? 5)))
        {
            return "Scale out of range";
        }

        return null;
    }

    public static Dictionary<int, double> BuildGraduateSharedVector(
        IReadOnlyDictionary<int, int> answers,
        IReadOnlyCollection<int> sharedQuestionIds)
    {
        return sharedQuestionIds.ToDictionary(id => id, id => answers.TryGetValue(id, out var value) ? (double)value : 0.0);
    }

    public static double ComputeSimilarityScore(
        IReadOnlyCollection<int> sharedQuestionIds,
        IReadOnlyCollection<int> multiChoiceIds,
        IReadOnlyDictionary<int, double> graduateAnswers,
        IReadOnlyCollection<SurveyResponse> responses)
    {
        var totalSimilarity = 0.0;
        var count = 0;

        foreach (var questionId in sharedQuestionIds)
        {
            var trackAnswers = responses
                .Where(r => r.QuestionId == questionId)
                .Select(r => (double)r.AnswerValue)
                .ToList();

            if (trackAnswers.Count == 0 || !graduateAnswers.ContainsKey(questionId))
            {
                continue;
            }

            var graduateValue = graduateAnswers[questionId];
            if (multiChoiceIds.Contains(questionId))
            {
                var mostCommonValue = trackAnswers
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                totalSimilarity += graduateValue == mostCommonValue ? 1.0 : 0.0;
            }
            else
            {
                var average = trackAnswers.Average();
                var diff = Math.Abs(graduateValue - average) / 4.0;
                totalSimilarity += 1.0 - Math.Min(diff, 1.0);
            }

            count++;
        }

        if (count == 0)
        {
            return 0;
        }

        return Math.Round((totalSimilarity / count) * 100, 1);
    }

    public static string MapEnvironment(int value)
    {
        return value switch
        {
            1 => "Remote",
            2 => "Office",
            3 => "Hybrid",
            _ => "N/A"
        };
    }

    public static string MapCompanySize(int value)
    {
        return value switch
        {
            1 => "Startup",
            2 => "Mid-size",
            3 => "Corporate",
            _ => "N/A"
        };
    }
}
