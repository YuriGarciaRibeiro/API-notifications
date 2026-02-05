# Domain Error Pattern

This folder contains custom domain errors that extend FluentResults to provide structured error handling with proper HTTP status codes.

## Available Errors

- **NotFoundError**: Resource not found (404)
- **ValidationError**: Validation failures (400)
- **ConflictError**: Resource already exists (409)
- **UnauthorizedError**: Authentication required (401)
- **ForbiddenError**: Authorization failed (403)
- **InternalError**: Unexpected errors (500)

## Usage Examples

### In a Service Method

```csharp
using FluentResults;
using NotificationSystem.Application.Common.Errors;

public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var user = await _repository.GetByIdAsync(id, cancellationToken);

    if (user is null)
        return Result.Fail<UserDto>(new NotFoundError("User", id));

    return Result.Ok(MapToDto(user));
}
```

### In a Command Handler

```csharp
public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    if (await _repository.EmailExistsAsync(request.Email, cancellationToken))
    {
        return Result.Fail<Guid>(
            new ConflictError("User", $"email '{request.Email}' already in use"));
    }

    var user = new User { /* ... */ };
    await _repository.AddAsync(user, cancellationToken);

    return Result.Ok(user.Id);
}
```

### Authorization Example

```csharp
public async Task<Result<UserDto>> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
{
    var currentUserId = _currentUserService.UserId;

    if (userId != currentUserId && !_currentUserService.IsAdmin)
    {
        return Result.Fail<UserDto>(
            new ForbiddenError("You can only update your own profile"));
    }

    // Update logic...
    return Result.Ok(updatedUser);
}
```

### Validation Example

```csharp
public async Task<Result<NotificationDto>> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Subject))
    {
        return Result.Fail<NotificationDto>(
            new ValidationError("Subject", "Subject is required"));
    }

    // Create logic...
    return Result.Ok(notificationDto);
}
```

## In Endpoints

All service methods returning `Result<T>` can be converted to HTTP responses using the extension:

```csharp
app.MapGet("/api/users/{id}", async (Guid id, IUserService userService, CancellationToken ct) =>
{
    var result = await userService.GetByIdAsync(id, ct);
    return result.ToIResult();
});

app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService, CancellationToken ct) =>
{
    var result = await userService.CreateAsync(request, ct);
    return result.ToIResult(StatusCodes.Status201Created);
});
```

## HTTP Response Mapping

The `ResultExtensions.ToIResult()` method automatically converts errors to RFC 7807 ProblemDetails format:

### 404 Not Found
```json
{
  "type": "https://api.example.com/errors/not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID 'abc123' was not found",
  "code": "NOT_FOUND",
  "traceId": "0HN5H2GV3P2GM:00000001"
}
```

### 400 Validation Error
```json
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "Subject is required",
  "code": "VALIDATION_ERROR",
  "errors": {
    "Subject": ["Subject is required", "Subject must be unique"]
  },
  "traceId": "0HN5H2GV3P2GM:00000001"
}
```

### 409 Conflict
```json
{
  "type": "https://api.example.com/errors/conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "User already exists: email 'john@example.com' already in use",
  "code": "CONFLICT",
  "traceId": "0HN5H2GV3P2GM:00000001"
}
```

## Creating Custom Errors

To create domain-specific errors, extend `DomainError`:

```csharp
namespace NotificationSystem.Application.Common.Errors;

public sealed class InvalidChannelError : DomainError
{
    public InvalidChannelError(string channelType)
        : base(
            code: "INVALID_CHANNEL",
            message: $"Channel type '{channelType}' is not supported",
            statusCode: 400)
    {
    }
}
```

Then use it in your code:

```csharp
if (!IsValidChannelType(request.ChannelType))
{
    return Result.Fail<NotificationDto>(
        new InvalidChannelError(request.ChannelType));
}
```
