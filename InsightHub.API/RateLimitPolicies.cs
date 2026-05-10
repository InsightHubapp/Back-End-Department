using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;
namespace InsightHub
{
    public static class RateLimitPolicies
    {
        public const string CareerQuiz = "CareerQuizPolicy";
        public const string Account = "AccountPolicy";

        public static void Register(RateLimiterOptions options)
        {
            options.AddPolicy(CareerQuiz, context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.Connection.RemoteIpAddress?.ToString()
                             ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            options.AddPolicy(Account, context =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? context.Connection.RemoteIpAddress?.ToString()
                             ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later."
                });
            };
        }
    }
}



