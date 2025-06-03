using System.Net;
using System.Text.Json;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.WebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ValidationException ex => CreateValidationErrorResponse(ex),
                NotFoundException ex => CreateNotFoundResponse(ex.Message),
                ForbiddenAccessException ex => CreateForbiddenResponse(ex.Message),
                UnauthorizedAccessException ex => CreateUnauthorizedResponse(ex.Message),
                _ => CreateInternalServerErrorResponse()
            };

            context.Response.StatusCode = response.StatusCode ?? 500;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static BaseResponse CreateValidationErrorResponse(ValidationException ex)
        {
            var errors = ex.Errors.SelectMany(x => x.Value).ToList();
            return BaseResponse.Failure("خطاهای اعتبارسنجی رخ داده است", errors, 400);
        }

        private static BaseResponse CreateNotFoundResponse(string message)
        {
            return BaseResponse.NotFound(message);
        }

        private static BaseResponse CreateForbiddenResponse(string message)
        {
            return BaseResponse.Failure(message, statusCode: 403);
        }

        private static BaseResponse CreateUnauthorizedResponse(string message)
        {
            return BaseResponse.Failure(message, statusCode: 401);
        }

        private static BaseResponse CreateInternalServerErrorResponse()
        {
            return BaseResponse.Failure("خطای داخلی سرور رخ داده است", statusCode: 500);
        }
    }
}
