namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for unexpected internal errors
/// </summary>
public sealed class InternalError : DomainError
{
    public InternalError(string message = "An unexpected error occurred", string? details = null)
        : base(
            code: "INTERNAL_ERROR",
            message: details != null ? $"{message}: {details}" : message,
            statusCode: 500)
    {
    }
}
