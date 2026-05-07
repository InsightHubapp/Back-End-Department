using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class InterviewQuizService : IInterviewQuizService
{
    private readonly IInterviewQuestionsSyncService _interviewQuestionsSyncService;
    private readonly AppDbContext _dbContext;

    public InterviewQuizService(
        IInterviewQuestionsSyncService interviewQuestionsSyncService,
        AppDbContext dbContext)
    {
        _interviewQuestionsSyncService = interviewQuestionsSyncService;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InterviewQVM>> GetByTrackAsync(string trackName, CancellationToken cancellationToken)
    {
        var questions = await QueryByTrackAsync(trackName, cancellationToken);

        if (questions.Count == 0)
        {
            await _interviewQuestionsSyncService.FetchAndStoreQuestions();
            questions = await QueryByTrackAsync(trackName, cancellationToken);
        }

        return questions;
    }

    public async Task<QuizResultVM> SubmitAnswersAsync(SubmitAnswersVM submission, CancellationToken cancellationToken)
    {
        var questionIds = submission.Answers.Select(a => a.QuestionId).Distinct().ToList();

        var questions = await _dbContext.Set<InterviewQuestion>()
            .AsNoTracking()
            .Include(q => q.Answers)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var score = 0;
        var correctAnswers = new List<CorrectAnswerVM>();

        foreach (var question in questions)
        {
            var userAnswer = submission.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
            if (correctAnswer == null)
            {
                continue;
            }

            var userOption = question.Answers.FirstOrDefault(a => a.Id == userAnswer?.Id);
            var isCorrect = userAnswer != null && userAnswer.Id == correctAnswer.Id;

            correctAnswers.Add(new CorrectAnswerVM
            {
                Question = question.Question,
                UserAnswer = userOption?.Option ?? string.Empty,
                CorrectAnswer = correctAnswer.Option ?? string.Empty,
                Explanation = question.Explanation ?? string.Empty,
                Difficulty = question.Difficulty ?? string.Empty,
                IsCorrect = isCorrect
            });

            if (isCorrect)
            {
                score++;
            }
        }

        return new QuizResultVM
        {
            Result = score,
            CorrectAnswers = correctAnswers
        };
    }

    private async Task<List<InterviewQVM>> QueryByTrackAsync(string trackName, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<InterviewQuestion>()
            .AsNoTracking()
            .Include(q => q.Answers)
            .Where(q => q.Category == trackName)
            .OrderBy(q => Guid.NewGuid())
            .Take(20)
            .Select(q => new InterviewQVM
            {
                Id = q.Id,
                Question = q.Question,
                Type = q.Type ?? string.Empty,
                Difficulty = q.Difficulty ?? string.Empty,
                Explanation = q.Explanation,
                Answers = q.Answers
                    .OrderBy(a => a.Number)
                    .Select(a => new InterviewQoptionVM
                    {
                        Id = a.Id,
                        Option = a.Option,
                        Number = a.Number,
                        IsCorrect = a.IsCorrect
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
