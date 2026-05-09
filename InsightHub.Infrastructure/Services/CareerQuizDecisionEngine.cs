using InsightHub.Domain.Entities;
using InsightHub.Domain.Enums;

namespace InsightHub.Infrastructure.Services;

public static class CareerQuizDecisionEngine
{
    private static readonly Dictionary<int, double> QuestionWeights = new()
    {
        { 111, 1.5 },  // Field Alignment
        { 112, 0.8 },  // Experience Level
        { 113, 0.7 },  // Work Environment
        { 114, 0.7 },  // Company Size
        { 115, 0.5 },  // Role Style
        { 116, 2.0 },  // Technical Level — الأهم
        { 117, 1.5 },  // Soft Skills
        { 118, 1.0 },  // Salary Satisfaction
        { 119, 1.0 },  // Work Life Balance
        { 120, 1.0 },  // Career Direction
    };

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
        var weightedSum = 0.0;
        var totalWeight = 0.0;

        foreach (var questionId in sharedQuestionIds)
        {
            var trackAnswers = responses
                .Where(r => r.QuestionId == questionId)
                .Select(r => (double)r.AnswerValue)
                .ToList();

            if (trackAnswers.Count == 0 || !graduateAnswers.ContainsKey(questionId))
                continue;

            var graduateValue = graduateAnswers[questionId];
            var weight = QuestionWeights.GetValueOrDefault(questionId, 1.0);

            double slotSimilarity;

            if (multiChoiceIds.Contains(questionId))
            {
                var mostCommonValue = trackAnswers
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                slotSimilarity = graduateValue == mostCommonValue ? 1.0 : 0.0;
            }
            else
            {
                var average = trackAnswers.Average();
                var diff = Math.Abs(graduateValue - average) / 4.0;
                slotSimilarity = 1.0 - Math.Min(diff, 1.0);
            }

            weightedSum += slotSimilarity * weight;
            totalWeight += weight;
        }

        if (totalWeight == 0)
            return 0;

        return Math.Round((weightedSum / totalWeight) * 100, 1);
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