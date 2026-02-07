namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for unexpected internal errors
/// </summary>
public sealed class InternalError(string message = "An unexpected error occurred", string? details = null) : DomainError(
        code: "INTERNAL_ERROR",
        message: details != null ? $"{message}: {details}" : message,
        statusCode: 500)
{
}
