namespace InsightHub.Application.ViewModels;

public class QuizApiResponse
{
    public bool Success { get; set; }
    public List<InterviewQuestionsVM> Data { get; set; } = new();
    public MetaData Meta { get; set; } = new();
}
