using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface ISurveyService
{
    Task<(bool Success, string? ErrorMessage, List<QuestionViewModel>? Data)> GetQuestionsAsync(string userId);
    Task<(bool Success, string? ErrorMessage)> SubmitAsync(string userId, SubmitSurveyViewModel model);
}
