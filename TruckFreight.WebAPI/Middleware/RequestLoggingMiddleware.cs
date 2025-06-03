namespace TruckFreight.WebAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            _logger.LogInformation("Starting request {Method} {Url} at {RequestTime}",
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation("Completed request {Method} {Url} with status {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
