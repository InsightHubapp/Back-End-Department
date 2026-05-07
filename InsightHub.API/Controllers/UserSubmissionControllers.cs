using InsightHub;
using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitPolicies.Account)]
public class UserSubmission : ControllerBase
{
    private readonly IUserSubmissionService _userSubmissionService;
    
    public UserSubmission(IUserSubmissionService userSubmissionService)
    {
        _userSubmissionService = userSubmissionService;
    }

    [Authorize]
    [HttpGet("EmploymentStatus")]
    public async Task<IActionResult> EmploymentStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile = await _userSubmissionService.UserSubmissionAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}

