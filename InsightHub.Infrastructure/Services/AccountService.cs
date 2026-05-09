using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InsightHub.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private const string OtpPrefix = "otp_";
    private const string FailedAttemptsPrefix = "fail_";
    private const int OtpLifetimeMinutes = 5;
    private const int MaxFailedAttempts = 5;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _db;
    public AccountService(

        IMemoryCache cache,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        AppDbContext db)
    {
        _cache = cache;
        _userManager = userManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    public async Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model)
    {
        var existingEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmail != null)
        {
            throw new InvalidOperationException("Email already exists");
        }

        if (model.IsEmployed && !model.TrackId.HasValue)
        {
            throw new InvalidOperationException("TrackId is required for employed users.");
        }

        var user = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Gender = model.Gender,
            BirthDate = model.BirthDate,
            Collage = model.Collage,
            IsEmployed = model.IsEmployed,
            TrackId = model.IsEmployed ? model.TrackId : null,
            YearsExperience = model.IsEmployed ? (model.YearsExperience ?? 0) : 0
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        return CreateAuthResponse(user, "User registered successfully");
    }

    public async Task<AuthResponseViewModel> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email)
            ?? await _userManager.FindByNameAsync(model.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        return CreateAuthResponse(user, string.Empty);
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<(bool Success, UserProfileViewModel? Profile, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, UserProfileViewModel profile)
    {
        var user = await _userManager.Users
            .Include(u => u.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return (false, null, null);

        user.FirstName = profile.FirstName ?? user.FirstName;
        user.LastName = profile.LastName ?? user.LastName;
        user.Gender = profile.Gender ?? user.Gender;
        user.BirthDate = profile.BirthDate ?? user.BirthDate;
        user.Collage = profile.Collage ?? user.Collage;
        user.YearsExperience = profile.YearsExperience ?? user.YearsExperience;

        if (profile.IsEmployed.HasValue && profile.IsEmployed != user.IsEmployed)
        {
            user.IsEmployed = profile.IsEmployed.Value;
            user.HasCompletedAssessment = false;
        }

        if (user.IsEmployed == false)
        {
            user.TrackId = null;
        }
        else if (profile.TrackName != null)
        {
            var track = await _db.Tracks
                .FirstOrDefaultAsync(t => t.Name == profile.TrackName);
            if (track != null)
                user.TrackId = track.Id;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        var updatedUser = await _userManager.Users
            .Include(u => u.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var viewModel = new UserProfileViewModel
        {
            Email = updatedUser!.Email!,
            FirstName = updatedUser.FirstName,
            LastName = updatedUser.LastName,
            Gender = updatedUser.Gender,
            BirthDate = updatedUser.BirthDate,
            Collage = updatedUser.Collage,
            IsEmployed = updatedUser.IsEmployed,
            YearsExperience = updatedUser.YearsExperience,
            TrackName = updatedUser.Track?.Name ?? "No Track Assigned"
        };

        return (true, viewModel, null);
    }

    public async Task<(bool Success, bool NotFound, bool Unauthorized, IEnumerable<string>? Errors)> DeleteAccountAsync(string? userId)
    {
        if (userId == null)
            return (false, false, true, null);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, true, false, null);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return (false, false, false, result.Errors.Select(e => e.Description));

        return (true, false, false, null);
    }

    public async Task<UserProfileViewModel?> GetProfileAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        return new UserProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Gender = user.Gender,
            BirthDate = user.BirthDate,
            Collage = user.Collage ?? string.Empty,
            IsEmployed = user.IsEmployed,
            YearsExperience = user.YearsExperience,
            TrackName = user.Track?.Name ?? "No Track Assigned"
        };
    }

    public async Task<(bool Success, string? Error)> RequestOtpAsync(string email)
    {
        if (await IsRateLimitedAsync(email))
            return (false, "Too many requests. Please wait before requesting a new OTP.");

        var otp = GenerateRawOtp();
        var hashed = HashOtp(otp);
        _cache.Set(OtpPrefix + email, hashed,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OtpLifetimeMinutes),
                Priority = CacheItemPriority.High
            });

        await SendOtpEmailAsync(email, otp);
        return (true, null);
    }

    public bool VerifyOtp(string email, string otp)
    {
        var failKey = FailedAttemptsPrefix + email;

        if (_cache.TryGetValue(failKey, out int failures) && failures >= MaxFailedAttempts)
            return false;

        if (_cache.TryGetValue(OtpPrefix + email, out string? storedHash))
        {
            var inputHash = HashOtp(otp);
            var match = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(inputHash),
                Encoding.UTF8.GetBytes(storedHash!));

            if (match)
            {
                _cache.Remove(OtpPrefix + email);
                _cache.Remove(failKey);
                return true;
            }
        }

        _cache.Set(failKey, (failures + 1), TimeSpan.FromMinutes(OtpLifetimeMinutes));
        return false;
    }

    private Task<bool> IsRateLimitedAsync(string email)
    {
        return Task.FromResult(false);
    }

    private string GenerateRawOtp()
    {
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return number.ToString("D6");
    }

    private string HashOtp(string otp)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(hashBytes);
    }

    private async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var fromEmail = _configuration["VerifierEmail:Email"];
        var appPassword = _configuration["VerifierEmail:AppPassword"];

        var digits = string.Join("", otp.Select(d =>
            $"<td style='width:48px;height:56px;background:#EFF6FF;border:1.5px solid #DBEAFE;" +
            $"border-radius:10px;text-align:center;vertical-align:middle;" +
            $"font-family:monospace;font-size:24px;font-weight:700;color:#2563EB;" +
            $"padding:0;margin:0 4px;'>{d}</td>"
        ));
        var htmlBody = $"""
             <!DOCTYPE html>
             <html>
             <body style="margin:0;padding:0;background:#F9FAFB;
               font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
               <table width="100%" cellpadding="0" cellspacing="0" style="padding:32px 16px;">
                 <tr><td align="center">
                   <table width="520" cellpadding="0" cellspacing="0"
                     style="background:#FFFFFF;border-radius:12px;border:1px solid #E5E7EB;">

                     <!-- Top bar -->
                     <tr><td style="background:#2563EB;height:4px;border-radius:12px 12px 0 0;font-size:0;">&nbsp;</td></tr>

                     <!-- Body -->
                     <tr><td style="padding:36px 36px 28px;">

                       <!-- Logo -->
                       <table cellpadding="0" cellspacing="0" style="margin-bottom:28px;">
                         <tr>
                           <td style="padding-left:10px;font-size:18px;font-weight:700;
                             color:#2563EB;vertical-align:middle;letter-spacing:-0.3px;">
                             InsightHub
                           </td>
                         </tr>
                       </table>

                       <hr style="border:none;border-top:1px solid #E5E7EB;margin:0 0 24px;"/>

                       <p style="font-size:15px;color:#111827;margin:0 0 8px;">
                         Hello, <strong>{toEmail}</strong>
                       </p>
                       <p style="font-size:14px;color:#6B7280;line-height:1.6;margin:0 0 28px;">
                         Use the code below to verify your email address.<br/>
                         This code is valid for <strong>5 minutes</strong> and can only be used once.
                       </p>

                       <!-- OTP label -->
                       <p style="font-size:11px;font-weight:600;color:#6B7280;
                         letter-spacing:1.5px;text-transform:uppercase;margin:0 0 12px;">
                         Verification code
                       </p>

                       <!-- OTP digits -->
                       <table cellpadding="0" cellspacing="4" style="margin-bottom:24px;">
                         <tr>{digits}</tr>
                       </table>

                       <!-- Expires -->
                       <table cellpadding="0" cellspacing="0" style="margin-bottom:28px;">
                         <tr>
                           <td style="width:6px;height:6px;background:#2563EB;
                             border-radius:50%;vertical-align:middle;"></td>
                           <td style="padding-left:6px;font-size:12px;
                             color:#2563EB;font-weight:500;">
                             Expires in 5 minutes
                           </td>
                         </tr>
                       </table>

                       <!-- Warning -->
                       <table width="100%" cellpadding="0" cellspacing="0"
                         style="background:#F9FAFB;border:1px solid #E5E7EB;border-radius:8px;">
                         <tr><td style="padding:12px 16px;font-size:12px;
                           color:#6B7280;line-height:1.7;">
                           If you didn't request this code, you can safely ignore this email.
                           Please do not share this code with anyone &mdash;
                           InsightHub will never ask for it.
                         </td></tr>
                       </table>

                     </td></tr>

                     <!-- Footer -->
                     <tr><td style="background:#F9FAFB;border-top:1px solid #E5E7EB;
                       border-radius:0 0 12px 12px;padding:16px 36px;
                       text-align:center;font-size:11px;color:#9CA3AF;line-height:1.8;">
                       This is an automated message from InsightHub &nbsp;&middot;&nbsp;
                       Please do not reply to this email
                     </td></tr>

                   </table>
                 </td></tr>
               </table>
             </body>
             </html>
             """;


        var message = new System.Net.Mail.MailMessage();
        message.From = new System.Net.Mail.MailAddress(fromEmail!, "InsightHub");
        message.To.Add(toEmail);
        message.Body = htmlBody;
        message.Subject = "Your InsightHub Verification Code";
        message.IsBodyHtml = true;

        using var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new System.Net.NetworkCredential(fromEmail, appPassword),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }


    private AuthResponseViewModel CreateAuthResponse(ApplicationUser user, string message)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("FirstName", user.FirstName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var keyText = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyText))
        {
            throw new InvalidOperationException("Jwt:Key is missing");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
        var audience = _configuration["Jwt:Audience"] ?? string.Empty;
        var expires = DateTime.UtcNow.AddDays(7);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponseViewModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expires,
            Username = user.UserName ?? user.Email ?? string.Empty,
            FirstName = user.FirstName,
            Message = message
        };
    }


}