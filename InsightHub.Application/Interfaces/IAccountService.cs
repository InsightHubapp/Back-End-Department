using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;

public interface IAccountService
{
    Task<AuthResponseViewModel> RegisterAsync(RegisterViewModel model);
    Task<AuthResponseViewModel> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<(bool Success, UserProfileViewModel? Profile, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, UserProfileViewModel profile);
    Task<(bool Success, bool NotFound, bool Unauthorized, IEnumerable<string>? Errors)> DeleteAccountAsync(string? userId);
    Task<UserProfileViewModel?> GetProfileAsync(string userId);
    Task<(bool Success, string? Error)> RequestOtpAsync(string email);
    bool VerifyOtp(string email, string otp);
}
