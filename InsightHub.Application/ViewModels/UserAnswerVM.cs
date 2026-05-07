namespace InsightHub.Application.ViewModels;

public class SubmitAnswersVM
{
    public List<UserAnswerVM> Answers { get; set; } = new();
}

public class UserAnswerVM
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
}

public class QuizResultVM
{
    public int Result { get; set; }
    public List<CorrectAnswerVM> CorrectAnswers { get; set; } = new();
}

public class CorrectAnswerVM
{
    public string Question { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
