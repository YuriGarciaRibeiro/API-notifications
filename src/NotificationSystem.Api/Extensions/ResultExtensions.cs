using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Common.Errors;

namespace NotificationSystem.Api.Extensions;

/// <summary>
/// Extension methods for converting FluentResults to HTTP responses
/// </summary>
public static class ResultExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an IResult (HTTP response).
    /// </summary>
    public static IResult ToIResult<T>(this Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: successStatusCode);
        }

        return BuildProblemResult(result.Errors);
    }

    /// <summary>
    /// Converts a Result (without value) to an IResult
    /// </summary>
    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return BuildProblemResult(result.Errors);
    }

    private static IResult BuildProblemResult(IReadOnlyList<IError> errors)
    {
        var context = _httpContextAccessor?.HttpContext;
        var path = context?.Request?.Path.Value ?? "";
        var traceId = context?.TraceIdentifier ?? "";

        // Extract domain error if present
        var domainError = errors.OfType<DomainError>().FirstOrDefault();
        var statusCode = domainError?.StatusCode ?? 400;
        var errorCode = domainError?.Code ?? "ERROR";

        // Group validation errors by field
        var validationErrors = errors
            .OfType<ValidationError>()
            .GroupBy(e => e.FieldName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray());

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetErrorTitle(errorCode, statusCode),
            Detail = domainError?.Message ?? string.Join("; ", errors.Select(e => e.Message)),
            Instance = path,
            Type = GetErrorType(errorCode)
        };

        problem.Extensions["traceId"] = traceId;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        if (validationErrors.Count > 0)
        {
            problem.Extensions["errors"] = validationErrors;
        }
        else if (errors.Count > 1)
        {
            problem.Extensions["errors"] = errors.Select(e => e.Message).ToArray();
        }

        return Results.Json(problem, statusCode: statusCode);
    }

    private static string GetErrorType(string errorCode) => errorCode switch
    {
        "NOT_FOUND" => "https://api.example.com/errors/not-found",
        "VALIDATION_ERROR" => "https://api.example.com/errors/validation-error",
        "CONFLICT" => "https://api.example.com/errors/conflict",
        "UNAUTHORIZED" => "https://api.example.com/errors/unauthorized",
        "FORBIDDEN" => "https://api.example.com/errors/forbidden",
        "INTERNAL_ERROR" => "https://api.example.com/errors/internal-error",
        _ => "https://api.example.com/errors/error"
    };

    private static string GetErrorTitle(string errorCode, int statusCode) => errorCode switch
    {
        "NOT_FOUND" => "Not Found",
        "VALIDATION_ERROR" => "Validation Error",
        "CONFLICT" => "Conflict",
        "UNAUTHORIZED" => "Unauthorized",
        "FORBIDDEN" => "Forbidden",
        "INTERNAL_ERROR" => "Internal Server Error",
        _ => statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            500 => "Internal Server Error",
            _ => "Operation Failed"
        }
    };
}
