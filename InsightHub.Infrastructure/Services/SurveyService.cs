using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Infrastructure.Services;

public class SurveyService : ISurveyService
{
    private readonly AppDbContext _db;

    public SurveyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? ErrorMessage, List<QuestionViewModel>? Data)> GetQuestionsAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("Unauthorized");

        if (!user.IsEmployed)
            throw new InvalidOperationException("User Must Be Employed.");

        if (user.HasCompletedAssessment)
            return (false, "You have already completed the assessment.", null);

        var questions = await _db.Questions
            .Include(q => q.Options)
            .Where(q => q.Id >= 101 && q.Id <= 120 && !q.IsCareerQuiz)
            .OrderBy(q => q.Order)
            .Select(q => new QuestionViewModel
            {
                Id = q.Id,
                Text = q.Text,
                Type = q.Type.ToString(),
                MaxValue = q.MaxValue,
                Options = q.Options.Select(o => new OptionViewModel
                {
                    Id = o.Id,
                    Text = o.Text,
                    NumericValue = o.NumericValue
                }).ToList()
            })
            .ToListAsync();

        return (true, "Success", questions);
    }

    public async Task<(bool Success, string? ErrorMessage)> SubmitAsync(string userId, SubmitSurveyViewModel model)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("Unauthorized");

        if (user.IsEmployed && user.TrackId == null)
            return (false, "Employed user must have a TrackId assigned.");

       

        var questionIds = model.Answers.Select(a => a.QuestionId).ToList();
        var validIds = await _db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => q.Id)
            .ToListAsync();

        if (validIds.Count != questionIds.Count)
            return (false, "One or more questions are invalid.");

        foreach (var answer in model.Answers)
        {
            var existing = await _db.SurveyResponses
                .FirstOrDefaultAsync(r => r.UserId == userId && r.QuestionId == answer.QuestionId);

            if (existing != null)
            {
                existing.AnswerValue = answer.AnswerValue;
            }
            else
            {
                _db.SurveyResponses.Add(new SurveyResponse
                {
                    UserId = userId,
                    QuestionId = answer.QuestionId,
                    AnswerValue = answer.AnswerValue
                });
            }
        }

        user.HasCompletedAssessment = true;
        await _db.SaveChangesAsync();

        return (true, null);
    }
}
