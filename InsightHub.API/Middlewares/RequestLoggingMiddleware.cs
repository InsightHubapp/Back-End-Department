using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InsightHu.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logFilePath = "logs.txt";

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userName = context.User.Identity?.Name ?? "Anonymous";

            var path = context.Request.Path;

            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var logLine = $"{time} | User: {userName} | Path: {path}";

            await File.AppendAllTextAsync(_logFilePath, logLine + Environment.NewLine);

          
            await _next(context);
        }
    }
}



