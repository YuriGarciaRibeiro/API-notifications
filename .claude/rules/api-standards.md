# API Standards & HTTP Conventions - Notification System

**Version**: .NET 10.0  
**Framework**: ASP.NET Core Minimal APIs  
**Last Update**: 02/05/2026

---

## 📋 Table of Contents

1. [Endpoint Structure](#endpoint-structure)
2. [HTTP Status Codes](#http-status-codes)
3. [Request/Response DTOs](#requestresponse-dtos)
4. [Polymorphic Channels](#polymorphic-channels)
5. [JWT Authentication](#jwt-authentication)
6. [Authorization Policies](#authorization-policies)
7. [Error Responses (RFC 7807)](#error-responses-rfc-7807)
8. [OpenAPI/Swagger](#openapiaswagger)
9. [Pagination](#pagination)
10. [CORS Configuration](#cors-configuration)

---

## Endpoint Structure

### Minimal API Routing
```csharp
// Program.cs
var app = builder.Build();

// Endpoint groups
var notificationGroup = app.MapGroup("/api/notifications")
    .WithName("Notifications")
    .WithOpenApi();

notificationGroup.MapGet("", GetNotifications)
    .WithName("Get Notifications")
    .WithDescription("Retrieve user's notifications with pagination")
    .RequireAuthorization();

notificationGroup.MapPost("", CreateNotification)
    .WithName("Create Notification")
    .WithDescription("Create a new notification")
    .Accepts<CreateNotificationRequest>("application/json")
    .Produces<CreatedResponse>(StatusCodes.Status201Created)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
    .RequireAuthorization();

notificationGroup.MapGet("{id}", GetNotificationById)
    .WithName("Get Notification")
    .WithDescription("Get notification details by ID")
    .RequireAuthorization();

notificationGroup.MapPut("{id}", UpdateNotification)
    .WithName("Update Notification")
    .WithDescription("Update notification")
    .RequireAuthorization("manage.notifications");

notificationGroup.MapDelete("{id}", DeleteNotification)
    .WithName("Delete Notification")
    .WithDescription("Delete notification")
    .RequireAuthorization("manage.notifications");
```

### Endpoint Handlers
```csharp
// GET /api/notifications
public async Task<IResult> GetNotifications(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromServices] IMediator mediator,
    HttpContext context,
    CancellationToken cancellationToken)
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var query = new GetUserNotificationsQuery(
        Guid.Parse(userId),
        new PaginationOptions { Page = page, PageSize = pageSize }
    );

    var result = await mediator.Send(query, cancellationToken);
    
    if (!result.IsSuccess)
        return Results.BadRequest(new ProblemDetails { Detail = result.Error });

    return Results.Ok(result.Data);
}

// POST /api/notifications
public async Task<IResult> CreateNotification(
    [FromBody] CreateNotificationRequest request,
    [FromServices] IMediator mediator,
    HttpContext context,
    CancellationToken cancellationToken)
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var command = new CreateNotificationCommand(
        Subject: request.Subject,
        Body: request.Body,
        RecipientId: request.RecipientId,
        Channels: request.Channels
    );

    var result = await mediator.Send(command, cancellationToken);
    
    if (!result.IsSuccess)
        return Results.BadRequest(new ProblemDetails { Detail = result.Error });

    return Results.Created($"/api/notifications/{result.Data.Id}", result.Data);
}

// GET /api/notifications/{id}
public async Task<IResult> GetNotificationById(
    [FromRoute] Guid id,
    [FromServices] IMediator mediator,
    HttpContext context,
    CancellationToken cancellationToken)
{
    var query = new GetNotificationByIdQuery(id);
    var result = await mediator.Send(query, cancellationToken);
    
    if (!result.IsSuccess)
        return Results.NotFound(new ProblemDetails { Detail = result.Error });

    return Results.Ok(result.Data);
}
```

---

## HTTP Status Codes

| Code | Status | Use Case |
|------|--------|----------|
| 200 | OK | Successful GET/PUT request |
| 201 | Created | Successful POST request |
| 202 | Accepted | Async operation accepted |
| 204 | No Content | DELETE successful, no response body |
| 400 | Bad Request | Validation error, malformed request |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | Authenticated but not authorized |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists (e.g., duplicate email) |
| 422 | Unprocessable Entity | Semantic error (valid JSON but invalid business logic) |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unhandled exception |
| 503 | Service Unavailable | External service down (RabbitMQ, Database) |

### Status Code Examples
```csharp
// 200 OK
return Results.Ok(notification);

// 201 Created
return Results.Created($"/api/notifications/{id}", notificationDto);

// 202 Accepted (async processing)
return Results.Accepted("/api/notifications", new { id = commandResult.Id });

// 204 No Content
return Results.NoContent();

// 400 Bad Request
return Results.BadRequest(new ProblemDetails
{
    Status = 400,
    Title = "Validation Error",
    Detail = "Subject is required"
});

// 404 Not Found
return Results.NotFound(new ProblemDetails
{
    Status = 404,
    Title = "Not Found",
    Detail = "Notification not found"
});

// 500 Internal Server Error
return Results.Problem(
    statusCode: 500,
    title: "Internal Server Error"
);
```

---

## Request/Response DTOs

### CreateNotificationRequest
```csharp
public record CreateNotificationRequest
{
    /// <summary>Notification subject/title</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Notification body/message</summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>Recipient user ID</summary>
    public Guid RecipientId { get; init; }

    /// <summary>Channels to send through (Email, Sms, Push)</summary>
    public NotificationChannel[] Channels { get; init; } = Array.Empty<NotificationChannel>();

    /// <summary>Optional: Template ID to use</summary>
    public Guid? TemplateId { get; init; }

    /// <summary>Optional: Custom metadata</summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
```

### NotificationDto (Response)
```csharp
public record NotificationDto
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public NotificationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
    
    /// <summary>Per-channel delivery status</summary>
    public ChannelStatusDto[] Channels { get; init; } = Array.Empty<ChannelStatusDto>();
}

public record ChannelStatusDto
{
    public NotificationChannel Channel { get; init; }
    public NotificationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? SentAt { get; init; }
}
```

### PaginatedResponse
```csharp
public record PagedResponse<T>
{
    public IEnumerable<T> Data { get; init; } = new List<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

## Polymorphic Channels

### Channel Enum
```csharp
public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    Push = 3
}
```

### Channel-Specific DTOs
```csharp
public record CreateNotificationRequest
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public NotificationChannel[] Channels { get; init; } = Array.Empty<NotificationChannel>();
    
    /// <summary>Channel-specific details (polymorphic)</summary>
    public EmailChannelData? EmailData { get; init; }
    public SmsChannelData? SmsData { get; init; }
    public PushChannelData? PushData { get; init; }
}

// Email channel specifics
public record EmailChannelData
{
    public string? ReplyTo { get; init; }
    public string[]? CcAddresses { get; init; }
    public string[]? BccAddresses { get; init; }
    public List<AttachmentData>? Attachments { get; init; }
}

// SMS channel specifics
public record SmsChannelData
{
    public int? MaxRetries { get; init; }
    public TimeSpan? RetryDelay { get; init; }
}

// Push channel specifics
public record PushChannelData
{
    public string? ImageUrl { get; init; }
    public Dictionary<string, string>? CustomData { get; init; }
    public string? ActionUrl { get; init; }
}

public record AttachmentData
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
}
```

### Response with Polymorphic Details
```csharp
public record NotificationDto
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Guid RecipientId { get; init; }
    public NotificationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    
    /// <summary>Per-channel status with polymorphic details</summary>
    public ChannelStatusDto[] Channels { get; init; } = Array.Empty<ChannelStatusDto>();
}

public abstract record ChannelStatusDto
{
    public NotificationChannel Channel { get; init; }
    public NotificationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? SentAt { get; init; }
}

public record EmailChannelStatusDto : ChannelStatusDto
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string? BouncedReason { get; init; }
}

public record SmsChannelStatusDto : ChannelStatusDto
{
    public string PhoneNumber { get; init; } = string.Empty;
    public int RetryCount { get; init; }
}

public record PushChannelStatusDto : ChannelStatusDto
{
    public string DeviceToken { get; init; } = string.Empty;
}
```

---

## JWT Authentication

### Token Request
```csharp
// POST /api/auth/login
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public string? RefreshToken { get; init; }
}
```

### Token Handler
```csharp
public async Task<IResult> Login(
    [FromBody] LoginRequest request,
    [FromServices] IAuthService authService,
    CancellationToken cancellationToken)
{
    var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
    
    if (!result.IsSuccess)
        return Results.Unauthorized();

    return Results.Ok(new TokenResponse
    {
        AccessToken = result.Data.AccessToken,
        ExpiresIn = result.Data.ExpiryMinutes * 60
    });
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "Secret": "your-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "NotificationSystem",
    "Audience": "NotificationSystemUsers",
    "ExpiryMinutes": 15
  }
}
```

### Middleware Setup
```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = configuration.GetSection("Jwt");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "")),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

---

## Authorization Policies

### Permission-Based Authorization
```csharp
// Custom attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

// Handler
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permission = context.User.FindFirst("permission")?.Value;

        if (permission == requirement.Permission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Registration
builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy("manage.notifications", policy =>
            policy.Requirements.Add(new PermissionRequirement("manage.notifications")));
        
        options.AddPolicy("manage.users", policy =>
            policy.Requirements.Add(new PermissionRequirement("manage.users")));
    })
    .AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

### Policy Usage
```csharp
var notificationGroup = app.MapGroup("/api/notifications")
    .RequireAuthorization("manage.notifications");

notificationGroup.MapDelete("{id}", DeleteNotification);

// Or inline
app.MapDelete("/api/notifications/{id}", DeleteNotification)
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
```

---

## Error Responses (RFC 7807)

### ProblemDetails Standard
```csharp
public record ProblemDetails
{
    /// <summary>RFC 7231 problem type URI</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Short description</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>HTTP status code</summary>
    [JsonPropertyName("status")]
    public int? Status { get; init; }

    /// <summary>Detailed explanation</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>Reference to specific occurrence</summary>
    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    /// <summary>Validation errors (extensions)</summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; init; }
}
```

### Error Response Examples
```json
// 400 Bad Request - Validation Error
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/notifications",
  "errors": {
    "Subject": ["Subject is required", "Subject cannot exceed 200 characters"],
    "Channels": ["At least one channel must be specified"]
  }
}

// 404 Not Found
{
  "type": "https://api.example.com/errors/not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "Notification 550e8400-e29b-41d4-a716-446655440000 not found",
  "instance": "/api/notifications/550e8400-e29b-41d4-a716-446655440000"
}

// 401 Unauthorized
{
  "type": "https://api.example.com/errors/unauthorized",
  "title": "Unauthorized",
  "status": 401,
  "detail": "The request requires authentication"
}

// 500 Internal Server Error
{
  "type": "https://api.example.com/errors/internal-error",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing your request",
  "instance": "/api/notifications"
}
```

### Exception Handler Middleware
```csharp
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        var problemDetails = new ProblemDetails();

        switch (exception)
        {
            case ValidationException ve:
                problemDetails = new ProblemDetails
                {
                    Type = "https://api.example.com/errors/validation-error",
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ve.Message,
                    Instance = context.Request.Path
                };
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case NotificationNotFoundException nfe:
                problemDetails = new ProblemDetails
                {
                    Type = "https://api.example.com/errors/not-found",
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = nfe.Message,
                    Instance = context.Request.Path
                };
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            default:
                logger.LogError(exception, "Unhandled exception");
                problemDetails = new ProblemDetails
                {
                    Type = "https://api.example.com/errors/internal-error",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred",
                    Instance = context.Request.Path
                };
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});
```

---

## OpenAPI/Swagger

### Configuration
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification System API",
        Version = "v1.0",
        Description = "Self-hosted multi-channel notification system",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Url = new Uri("https://example.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification System API v1");
        options.RoutePrefix = "swagger";
    });
}
```

### Endpoint Documentation
```csharp
app.MapPost("/api/notifications", CreateNotification)
    .WithName("Create Notification")
    .WithDescription("Create a new notification and queue for delivery")
    .WithOpenApi()
    .Accepts<CreateNotificationRequest>("application/json")
    .Produces<NotificationDto>(StatusCodes.Status201Created)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
    .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
    .RequireAuthorization();
```

---

## Pagination

### Query Parameters
```csharp
public record GetNotificationsQuery(
    [FromQuery(Name = "page")] int Page = 1,
    [FromQuery(Name = "pageSize")] int PageSize = 20,
    [FromQuery(Name = "sortBy")] string SortBy = "createdAt",
    [FromQuery(Name = "sortOrder")] string SortOrder = "desc"
);
```

### Paginated Response
```csharp
public record PagedResponse<T>
{
    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; init; } = new List<T>();

    [JsonPropertyName("pagination")]
    public PaginationMetadata Pagination { get; init; } = new();
}

public record PaginationMetadata
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; init; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; init; }
}
```

### Example Response
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "subject": "Welcome to our platform",
      "status": "Sent"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

## CORS Configuration

### Setup
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page-Count");
    });
});

app.UseCors("AllowFrontend");
```

### Configuration
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://example.com"
    ]
  }
}
```

### Production CORS
```csharp
// ❌ WRONG - AllowAnyOrigin
options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin());

// ✅ CORRECT - Specific origins
options.AddPolicy("Production", policy =>
{
    policy
        .WithOrigins("https://app.example.com", "https://admin.example.com")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});
```

---

## Summary Checklist

- ✅ Meaningful HTTP status codes (200, 201, 400, 401, 404, 500)
- ✅ RFC 7807 ProblemDetails for errors
- ✅ Request/Response DTOs with validation
- ✅ JWT Bearer authentication
- ✅ Permission-based authorization
- ✅ Polymorphic channel data handling
- ✅ Pagination with metadata
- ✅ OpenAPI/Swagger documentation
- ✅ Proper CORS configuration
- ✅ Consistent naming conventions
