namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for authorization failures
/// </summary>
public sealed class ForbiddenError(string message = "You do not have permission to perform this action") : DomainError(
        code: "FORBIDDEN",
        message: message,
        statusCode: 403)
{
}
