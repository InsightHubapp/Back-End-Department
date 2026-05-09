using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitPolicies.Account)]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _accountService.RegisterAsync(model);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _accountService.LoginAsync(model);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [Authorize]
    [DisableRateLimiting]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return Ok("Logged out successfully");
    }

    [DisableRateLimiting]
    [HttpPost("EmailExistance")]
    public async Task<IActionResult> EmailExistance([FromBody] EmailExistance email)
    {
        var exists = await _accountService.EmailExistsAsync(email.Email);
        if (!exists)
        {
            return NotFound("This email doesn't exist.");
        }

        return Ok("Account exists");
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile = await _accountService.GetProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileViewModel profile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (success, userProfile, errors) = await _accountService.UpdateProfileAsync(userId!, profile);

        if (userProfile == null && errors == null)
            return NotFound(new { Message = "User not found" });

        if (!success)
            return BadRequest(new { Errors = errors });

        return Ok(userProfile);
    }

    [Authorize]
    [DisableRateLimiting]
    [HttpDelete("DeleteAccount")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (success, notFound, unauthorized, errors) = await _accountService.DeleteAccountAsync(userId);

        if (unauthorized) return Unauthorized();
        if (notFound) return NotFound();
        if (!success) return BadRequest(new { Errors = errors });

        return Ok("Account deleted successfully");
    }


    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOTP([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, error) = await _accountService.RequestOtpAsync(request.Email);
        if (!success)
            return StatusCode(429, new { Message = error });

        return Ok(new { Message = "If this email is registered, an OTP has been sent." });
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public IActionResult VerifyOtp([FromBody] VerifyOTPVM model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var isValid = _accountService.VerifyOtp(model.Email, model.Otp);
        if (!isValid)
            return BadRequest(new { Message = "Invalid or expired OTP." });

        return Ok(new { Message = "OTP verified successfully." });
    }

}
