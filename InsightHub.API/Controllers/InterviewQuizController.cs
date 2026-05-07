using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewQuizController : ControllerBase
{
    private readonly IInterviewQuizService _interviewQuizService;

    public InterviewQuizController(IInterviewQuizService interviewQuizService)
    {
        _interviewQuizService = interviewQuizService;
    }

    [HttpPost("Questions")]
    public async Task<IActionResult> GetByTrack([FromBody] QuizCategoryVM trackName)
    {
        if (string.IsNullOrWhiteSpace(trackName.TrackName))
        {
            return BadRequest(new { message = "TrackName is required." });
        }

        var questions = await _interviewQuizService.GetByTrackAsync(trackName.TrackName, HttpContext.RequestAborted);
        if (questions.Count == 0)
        {
            return NotFound(new { message = $"No questions found for track: {trackName.TrackName}" });
        }

        return Ok(questions);
    }

    [HttpPost("Submit")]
    public async Task<IActionResult> SubmitAnswers([FromBody] SubmitAnswersVM submission)
    {
        if (submission?.Answers == null || submission.Answers.Count == 0)
        {
            return BadRequest(new { message = "No answers provided." });
        }

        var result = await _interviewQuizService.SubmitAnswersAsync(submission, HttpContext.RequestAborted);
        return Ok(result);
    }
}
