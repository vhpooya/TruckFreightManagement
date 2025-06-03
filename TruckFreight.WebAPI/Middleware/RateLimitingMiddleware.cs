using System.Collections.Concurrent;

namespace TruckFreight.WebAPI.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, DateTime[]> _requestLog = new();
        private readonly int _requestLimit = 100; // requests per minute
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            
            if (IsRateLimited(clientId))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            LogRequest(clientId);
            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID first, then IP address
            var userId = context.User?.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(userId))
                return $"user:{userId}";

            return $"ip:{context.Connection.RemoteIpAddress}";
        }

        private bool IsRateLimited(string clientId)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.Subtract(_timeWindow);

            if (!_requestLog.TryGetValue(clientId, out var requests))
                return false;

            var recentRequests = requests.Where(r => r > windowStart).ToArray();
            _requestLog[clientId] = recentRequests;

            return recentRequests.Length >= _requestLimit;
        }

        private void LogRequest(string clientId)
        {
            var now = DateTime.UtcNow;
            _requestLog.AddOrUpdate(clientId, 
                new[] { now }, 
                (key, existing) => existing.Append(now).ToArray());
        }
    }
}
