using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace NotificationSystem.Api.Extensions;

public static class ResultExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static IResult ToIResult<T>(this Result<T> result, int? successStatusCode = null)
    {
        if (result.IsSuccess)
        {
            var statusCode = successStatusCode ?? StatusCodes.Status200OK;
            return Results.Json(result.Value, statusCode: statusCode);
        }

        return BuildProblemResult(result.Errors);
    }

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
        var traceId = context?.TraceIdentifier;

        var firstError = errors.FirstOrDefault();
        var statusCode = firstError?.Metadata.TryGetValue("StatusCode", out var codeObj) == true && codeObj is int code
            ? code
            : 400;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Operation failed",
            Detail = string.Join("; ", errors.Select(e => e.Message)),
            Instance = path
        };

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            problem.Extensions["traceId"] = traceId;
        }

        problem.Extensions["timestamp"] = DateTime.UtcNow;
        problem.Extensions["errors"] = errors.Select(e => new
        {
            e.Message,
            Metadata = e.Metadata
        }).ToList();

        return Results.Problem(
            problem.Detail,
            statusCode: statusCode,
            title: problem.Title,
            instance: problem.Instance,
            extensions: problem.Extensions);
    }
}
