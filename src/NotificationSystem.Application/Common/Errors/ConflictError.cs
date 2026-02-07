namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error when a resource already exists (conflict)
/// </summary>
public sealed class ConflictError(string resourceName, string details) : DomainError(
        code: "CONFLICT",
        message: $"{resourceName} already exists: {details}",
        statusCode: 409)
{
}
