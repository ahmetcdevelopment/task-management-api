using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TaskManagement.API.Middlewares;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            await HandleValidationExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = new ValidationErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed",
            Errors = exception.Errors.Select(error => new ValidationError
            {
                PropertyName = error.PropertyName,
                ErrorMessage = error.ErrorMessage,
                AttemptedValue = error.AttemptedValue?.ToString()
            }).ToList(),
            Timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ValidationErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? AttemptedValue { get; set; }
}