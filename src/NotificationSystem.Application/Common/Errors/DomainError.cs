using FluentResults;

namespace NotificationSystem.Application.Common.Errors;

/// <summary>
/// Base class for domain-specific errors
/// </summary>
public abstract class DomainError : Error
{
    /// <summary>
    /// Error code for identification
    /// </summary>
    public string Code { get; protected set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; protected set; }

    protected DomainError(string code, string message, int statusCode = 400)
    {
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }
}
