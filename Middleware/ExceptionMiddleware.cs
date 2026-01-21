using EmployeeAdminPortal.Exceptions;
using EmployeeAdminPortal.Models;
using System.Text.Json;

namespace EmployeeAdminPortal.Middleware
{

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Resource not found: {Message}", ex.Message);

                await SendErrorResponse(
                    context,
                    statusCode: 404,
                    message: ex.Message,
                    errors: null
                );
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed: {Message}", ex.Message);

                await SendErrorResponse(
                    context,
                    statusCode: 400,
                    message: "Validation failed.",
                    errors: ex.Errors
                );
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request: {Message}", ex.Message);

                await SendErrorResponse(
                    context,
                    statusCode: 400,
                    message: ex.Message,
                    errors: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);

                await SendErrorResponse(
                    context,
                    statusCode: 500,
                    message: "Something went wrong. Please try again later.",
                    errors: null
                );
            }
        }
        private async Task SendErrorResponse(
            HttpContext context,
            int statusCode,
            string message,
            List<ErrorDetail>? errors)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var errorResponse = new ErrorResponse
            {
                Message = message,
                Errors = errors ?? new List<ErrorDetail>()
            };
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
