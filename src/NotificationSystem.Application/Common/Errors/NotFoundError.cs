namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error when a resource is not found
/// </summary>
public sealed class NotFoundError : DomainError
{
    public NotFoundError(string resourceName, Guid resourceId)
        : base(
            code: "NOT_FOUND",
            message: $"{resourceName} with ID '{resourceId}' was not found",
            statusCode: 404)
    {
    }

    public NotFoundError(string resourceName, string identifier)
        : base(
            code: "NOT_FOUND",
            message: $"{resourceName} '{identifier}' was not found",
            statusCode: 404)
    {
    }
}
