namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for authentication failures
/// </summary>
public sealed class UnauthorizedError(string message = "Authentication required") : DomainError(
        code: "UNAUTHORIZED",
        message: message,
        statusCode: 401)
{
}
