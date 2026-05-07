using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface IInterviewQuestionsSyncService
{
    Task<List<InterviewQuestionsVM>> GetQuestionsFromApi(string[] tags, string difficulty, int offset = 0);
    Task FetchAndStoreQuestions();
}
