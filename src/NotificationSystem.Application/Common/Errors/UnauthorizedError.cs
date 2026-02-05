namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for authentication failures
/// </summary>
public sealed class UnauthorizedError : DomainError
{
    public UnauthorizedError(string message = "Authentication required")
        : base(
            code: "UNAUTHORIZED",
            message: message,
            statusCode: 401)
    {
    }
}
