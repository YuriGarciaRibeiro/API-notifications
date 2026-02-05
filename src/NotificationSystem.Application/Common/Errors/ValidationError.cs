namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Error for validation failures
/// </summary>
public sealed class ValidationError : DomainError
{
    public string FieldName { get; }

    public ValidationError(string fieldName, string message)
        : base(
            code: "VALIDATION_ERROR",
            message: message,
            statusCode: 400)
    {
        FieldName = fieldName;
    }
}
