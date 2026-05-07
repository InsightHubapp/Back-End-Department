using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsightHub.Infrastructure.Services
{
    public class UserSubmissionService : IUserSubmissionService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSubmissionService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserSubmissionVeiwModel?> UserSubmissionAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Track)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return new UserSubmissionVeiwModel
            {
                IsEmployed = user.IsEmployed,
                HasCompletedAssessment = user.HasCompletedAssessment
            };
        }
    }
}
