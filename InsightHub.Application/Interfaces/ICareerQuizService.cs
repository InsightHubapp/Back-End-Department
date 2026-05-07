using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface ICareerQuizService
{
    Task<List<CareerQuizQuestionViewModel>> GetQuestionsAsync();
    Task<UnifiedFullMatchResultViewModel> GetMatchResultAsync(string userId, SubmitFullMatchViewModel model);
    Task<CareerQuizStoredResultViewModel?> GetStoredResultAsync(string userId);
}
