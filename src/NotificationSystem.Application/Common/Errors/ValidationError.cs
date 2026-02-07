namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for validation failures
/// </summary>
public sealed class ValidationError(string fieldName, string message) : DomainError(
        code: "VALIDATION_ERROR",
        message: message,
        statusCode: 400)
{
    public string FieldName { get; } = fieldName;
}
