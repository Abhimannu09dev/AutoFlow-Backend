using System.Text.Json;
using AutoFlow_Backend.Application.Common;

namespace AutoFlow_Backend.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = MapException(exception);

        _logger.LogError(exception, "Unhandled exception. StatusCode: {StatusCode}", statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object?>
        {
            Status = false,
            Message = message,
            Data = null
        };

        var payload = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(payload);
    }

    private static (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badRequest => (
                StatusCodes.Status400BadRequest,
                string.IsNullOrWhiteSpace(badRequest.Message) ? "Bad request." : badRequest.Message),

            KeyNotFoundException notFound => (
                StatusCodes.Status404NotFound,
                string.IsNullOrWhiteSpace(notFound.Message) ? "Resource not found." : notFound.Message),

            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                string.IsNullOrWhiteSpace(argumentException.Message) ? "Invalid request." : argumentException.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.")
        };
    }
}
