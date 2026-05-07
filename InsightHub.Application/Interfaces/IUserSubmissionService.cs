using InsightHub.Application.ViewModels;

namespace InsightHub.Application.Interfaces;
 public interface IUserSubmissionService
 {
    Task<UserSubmissionVeiwModel?> UserSubmissionAsync(string userId);


 }

