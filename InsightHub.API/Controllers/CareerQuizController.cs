using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitPolicies.CareerQuiz)]
[Authorize]
public class CareerQuizController : ControllerBase
{
    private readonly ICareerQuizService _careerQuizService;

    public CareerQuizController(ICareerQuizService careerQuizService)
    {
        _careerQuizService = careerQuizService;
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions()
    {
        var questions = await _careerQuizService.GetQuestionsAsync();
        return Ok(questions);
    }

    [HttpPost("full-match")]
    public async Task<IActionResult> FullMatch([FromBody] SubmitFullMatchViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var submittedIds = model.Answers.Select(a => a.QuestionId).ToHashSet();
        var missingShared = Enumerable.Range(111, 10).Where(id => !submittedIds.Contains(id)).ToList();
        var missingQuiz = Enumerable.Range(121, 25).Where(id => !submittedIds.Contains(id)).ToList();

        if (missingShared.Count > 0 || missingQuiz.Count > 0)
        {
            return BadRequest(new
            {
                message = "Cannot process full-match. Some questions are missing.",
                missingSharedInsights = missingShared,
                missingCareerQuiz = missingQuiz
            });
        }

        try
        {
            var results = await _careerQuizService.GetMatchResultAsync(userId, model);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
        }
    }

    [HttpGet("result")]
    public async Task<IActionResult> GetResult()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _careerQuizService.GetStoredResultAsync(userId);
        if (result == null)
        {
            return NotFound(new { message = "No result found. Please complete the career quiz first." });
        }

        return Ok(result);
    }
}
