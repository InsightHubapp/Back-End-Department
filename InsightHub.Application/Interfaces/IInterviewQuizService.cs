using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface IInterviewQuizService
{
    Task<IReadOnlyList<InterviewQVM>> GetByTrackAsync(string trackName, CancellationToken cancellationToken);
    Task<QuizResultVM> SubmitAnswersAsync(SubmitAnswersVM submission, CancellationToken cancellationToken);
}
