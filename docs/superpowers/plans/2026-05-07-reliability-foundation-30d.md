# Reliability Foundation (30 Days) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish a production-reliability baseline in 30 days with health visibility, safer runtime defaults, CI quality gates, and repeatable operational validation.

**Architecture:** Keep the existing Clean Architecture and Minimal API structure, adding a thin reliability slice in API startup, configuration binding, and tests. Prefer incremental hardening over broad refactors: first make health and failure modes visible, then enforce pre-merge validation and operational checks.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, xUnit, WebApplicationFactory, RabbitMQ.Client, EF Core, GitHub Actions.

---

## File Structure and Responsibilities

### Existing files to modify

- `src/NotificationSystem.Api/Program.cs`
- `src/NotificationSystem.Api/DependencyInjection.cs`
- `src/NotificationSystem.Api/NotificationSystem.Api.csproj`
- `src/NotificationSystem.Api/appsettings.json`
- `src/NotificationSystem.Api/appsettings.Development.json`
- `src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs`
- `src/NotificationSystem.Application/Services/DeadLetterQueueService.cs`
- `src/Consumers/NotificationSystem.Consumer.Email/Program.cs`
- `src/Consumers/NotificationSystem.Consumer.Sms/Program.cs`
- `src/Consumers/NotificationSystem.Consumer.Push/Program.cs`
- `src/Consumers/NotificationSystem.Consumer.Bulk/Program.cs`
- `NotificationSystem.slnx`
- `README.md`

### New files to create

- `src/NotificationSystem.Api/Health/RabbitMqHealthCheck.cs`
- `src/NotificationSystem.Api/Configuration/CorsSettings.cs`
- `src/NotificationSystem.Api/Extensions/HealthCheckResponseWriter.cs`
- `tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj`
- `tests/NotificationSystem.Api.Tests/Infrastructure/NotificationApiFactory.cs`
- `tests/NotificationSystem.Api.Tests/Health/HealthEndpointsTests.cs`
- `tests/NotificationSystem.Api.Tests/Security/CorsPolicyTests.cs`
- `tests/NotificationSystem.Api.Tests/Consumers/MessageProcessingMiddlewareLoggingTests.cs`
- `.github/workflows/ci.yml`
- `scripts/smoke/health-smoke.sh`
- `docs/operations/reliability-baseline.md`

---

### Task 1: Build Test Harness for Reliability Work

**Files:**
- Create: `tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj`
- Create: `tests/NotificationSystem.Api.Tests/Infrastructure/NotificationApiFactory.cs`
- Create: `tests/NotificationSystem.Api.Tests/Health/HealthEndpointsTests.cs`
- Modify: `src/NotificationSystem.Api/Program.cs`
- Modify: `NotificationSystem.slnx`

- [ ] **Step 1: Write failing integration test for liveness endpoint**

```csharp
// tests/NotificationSystem.Api.Tests/Health/HealthEndpointsTests.cs
using System.Net;
using System.Net.Http.Json;
using NotificationSystem.Api.Tests.Infrastructure;

namespace NotificationSystem.Api.Tests.Health;

public class HealthEndpointsTests(NotificationApiFactory factory) : IClassFixture<NotificationApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_Liveness_Should_Return200()
    {
        var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(payload);
        Assert.True(payload!.ContainsKey("status"));
    }
}
```

- [ ] **Step 2: Add test project skeleton and factory**

```xml
<!-- tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\NotificationSystem.Api\NotificationSystem.Api.csproj" />
  </ItemGroup>
</Project>
```

```csharp
// tests/NotificationSystem.Api.Tests/Infrastructure/NotificationApiFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace NotificationSystem.Api.Tests.Infrastructure;

public sealed class NotificationApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DisableDatabaseInitialization"] = "true",
                ["Jwt:Secret"] = "testing-secret-key-with-at-least-32-chars",
                ["Jwt:Issuer"] = "NotificationSystem",
                ["Jwt:Audience"] = "NotificationSystem"
            });
        });
    }
}
```

- [ ] **Step 3: Make API startup testable (skip DB init in Testing only)**

```csharp
// src/NotificationSystem.Api/Program.cs
using NotificationSystem.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilog();

try
{
    Log.Information("Starting NotificationSystem API");

    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);

    var app = builder.Build();

    var disableDbInitialization = app.Configuration.GetValue<bool>("DisableDatabaseInitialization");
    if (!disableDbInitialization)
    {
        await app.InitializeDatabaseAsync();
    }

    app.ConfigureHttpPipeline();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
```

- [ ] **Step 4: Add tests project to solution and run failing/passing loop**

Run:

```bash
dotnet sln NotificationSystem.slnx add tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj
```

Run:

```bash
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -v minimal --filter "FullyQualifiedName~HealthEndpointsTests"
```

Expected before health endpoint implementation: FAIL with `404 NotFound`.

Expected after Task 2 implementation: PASS.

- [ ] **Step 5: Commit**

```bash
git add NotificationSystem.slnx src/NotificationSystem.Api/Program.cs tests/NotificationSystem.Api.Tests
git commit -m "test: add API integration harness for reliability baseline"
```

---

### Task 2: Add Liveness/Readiness Endpoints with DB and RabbitMQ Checks

**Files:**
- Modify: `src/NotificationSystem.Api/NotificationSystem.Api.csproj`
- Create: `src/NotificationSystem.Api/Health/RabbitMqHealthCheck.cs`
- Create: `src/NotificationSystem.Api/Extensions/HealthCheckResponseWriter.cs`
- Modify: `src/NotificationSystem.Api/DependencyInjection.cs`
- Modify: `tests/NotificationSystem.Api.Tests/Health/HealthEndpointsTests.cs`

- [ ] **Step 1: Write failing readiness test**

```csharp
[Fact]
public async Task Get_Readiness_Should_Return200_Or503_WithStatusPayload()
{
    var response = await _client.GetAsync("/health/ready");

    Assert.True(
        response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
        $"Unexpected status code: {response.StatusCode}");

    var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
    Assert.NotNull(payload);
    Assert.True(payload!.ContainsKey("status"));
    Assert.True(payload.ContainsKey("checks"));
}
```

- [ ] **Step 2: Add health-check dependencies**

```xml
<!-- src/NotificationSystem.Api/NotificationSystem.Api.csproj -->
<ItemGroup>
  <PackageReference Include="Hangfire.AspNetCore" Version="1.8.14" />
  <PackageReference Include="Hangfire.Core" Version="1.8.14" />
  <PackageReference Include="Hangfire.PostgreSql" Version="1.20.9" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.1">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="10.0.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
  <PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="10.0.1" />
</ItemGroup>
```

- [ ] **Step 3: Implement RabbitMQ health check and JSON response writer**

```csharp
// src/NotificationSystem.Api/Health/RabbitMqHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Configuration;
using RabbitMQ.Client;

namespace NotificationSystem.Api.Health;

public sealed class RabbitMqHealthCheck(IOptions<RabbitMqSettings> options) : IHealthCheck
{
    private readonly RabbitMqSettings _settings = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ connection established");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ unreachable", ex);
        }
    }
}
```

```csharp
// src/NotificationSystem.Api/Extensions/HealthCheckResponseWriter.cs
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace NotificationSystem.Api.Extensions;

public static class HealthCheckResponseWriter
{
    public static async Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    durationMs = entry.Value.Duration.TotalMilliseconds,
                    error = entry.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
```

- [ ] **Step 4: Register and map health endpoints**

```csharp
// src/NotificationSystem.Api/DependencyInjection.cs (inside AddApiServices)
services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("postgres", tags: ["ready"])
    .AddCheck<NotificationSystem.Api.Health.RabbitMqHealthCheck>("rabbitmq", tags: ["ready"]);
```

```csharp
// src/NotificationSystem.Api/DependencyInjection.cs (inside ConfigureHttpPipeline)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
});
```

- [ ] **Step 5: Run tests and commit**

Run:

```bash
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -v minimal --filter "FullyQualifiedName~HealthEndpointsTests"
```

Expected: PASS.

Commit:

```bash
git add src/NotificationSystem.Api tests/NotificationSystem.Api.Tests
git commit -m "feat: add liveness and readiness health endpoints"
```

---

### Task 3: Harden CORS Configuration for Production Safety

**Files:**
- Create: `src/NotificationSystem.Api/Configuration/CorsSettings.cs`
- Modify: `src/NotificationSystem.Api/DependencyInjection.cs`
- Modify: `src/NotificationSystem.Api/appsettings.json`
- Modify: `src/NotificationSystem.Api/appsettings.Development.json`
- Create: `tests/NotificationSystem.Api.Tests/Security/CorsPolicyTests.cs`

- [ ] **Step 1: Write failing CORS policy test**

```csharp
// tests/NotificationSystem.Api.Tests/Security/CorsPolicyTests.cs
using System.Net;
using NotificationSystem.Api.Tests.Infrastructure;

namespace NotificationSystem.Api.Tests.Security;

public class CorsPolicyTests(NotificationApiFactory factory) : IClassFixture<NotificationApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Preflight_From_Unknown_Origin_Should_Not_Return_AccessControlAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/notifications");
        request.Headers.Add("Origin", "https://evil.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await _client.SendAsync(request);

        Assert.True(response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.MethodNotAllowed);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
```

- [ ] **Step 2: Introduce typed CORS settings**

```csharp
// src/NotificationSystem.Api/Configuration/CorsSettings.cs
namespace NotificationSystem.Api.Configuration;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = [];
}
```

- [ ] **Step 3: Replace AllowAnyOrigin with config-driven policy**

```csharp
// src/NotificationSystem.Api/DependencyInjection.cs
using NotificationSystem.Api.Configuration;

public static IServiceCollection AddCorsPolicy(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));

    var corsSettings = configuration
        .GetSection(CorsSettings.SectionName)
        .Get<CorsSettings>() ?? new CorsSettings();

    if (!environment.IsDevelopment() && corsSettings.AllowedOrigins.Length == 0)
    {
        throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");
    }

    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (corsSettings.AllowedOrigins.Length == 0)
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                return;
            }

            policy.WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    return services;
}
```

- [ ] **Step 4: Align configuration files**

```json
// src/NotificationSystem.Api/appsettings.json
{
  "Cors": {
    "AllowedOrigins": []
  }
}
```

```json
// src/NotificationSystem.Api/appsettings.Development.json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000",
      "https://localhost:5173",
      "https://localhost:3000"
    ]
  }
}
```

- [ ] **Step 5: Run tests and commit**

Run:

```bash
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -v minimal --filter "FullyQualifiedName~CorsPolicyTests"
```

Expected: PASS.

Commit:

```bash
git add src/NotificationSystem.Api tests/NotificationSystem.Api.Tests
git commit -m "feat: enforce config-driven CORS policy"
```

---

### Task 4: Remove Sync-over-Async RabbitMQ Initialization Risks

**Files:**
- Modify: `src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs`
- Modify: `src/NotificationSystem.Application/Services/DeadLetterQueueService.cs`
- Modify: `src/NotificationSystem.Api/DependencyInjection.cs`

- [ ] **Step 1: Write failing unit test for async initialization contract**

```csharp
// tests/NotificationSystem.Api.Tests/Health/HealthEndpointsTests.cs (append)
[Fact]
public async Task Readiness_Should_Not_Block_When_RabbitMQ_Is_Down()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    var response = await _client.GetAsync("/health/ready", cts.Token);

    Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);
}
```

- [ ] **Step 2: Refactor publisher to lazy async channel initialization**

```csharp
// src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs
private readonly SemaphoreSlim _initLock = new(1, 1);
private IConnection? _connection;
private IChannel? _channel;

private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
{
    if (_channel is not null)
        return;

    await _initLock.WaitAsync(cancellationToken);
    try
    {
        if (_channel is not null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.Username,
            Password = _settings.Password,
            Port = _settings.Port
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await DeclareQueuesAsync(cancellationToken);
    }
    finally
    {
        _initLock.Release();
    }
}

public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
{
    await EnsureInitializedAsync(cancellationToken);
    // publish using _channel!
}
```

- [ ] **Step 3: Convert DLQ service to hosted lifecycle init (no GetAwaiter().GetResult())**

```csharp
// src/NotificationSystem.Application/Services/DeadLetterQueueService.cs
public class DeadLetterQueueService : IDeadLetterQueueService, IHostedService, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Register lifecycle correctly and validate**

```csharp
// src/NotificationSystem.Api/DependencyInjection.cs
services.AddSingleton<DeadLetterQueueService>();
services.AddSingleton<IDeadLetterQueueService>(sp => sp.GetRequiredService<DeadLetterQueueService>());
services.AddHostedService(sp => sp.GetRequiredService<DeadLetterQueueService>());
```

Run:

```bash
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -v minimal --filter "FullyQualifiedName~Readiness_Should_Not_Block_When_RabbitMQ_Is_Down"
```

Expected: PASS (fast response, no startup deadlock).

- [ ] **Step 5: Commit**

```bash
git add src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs src/NotificationSystem.Application/Services/DeadLetterQueueService.cs src/NotificationSystem.Api/DependencyInjection.cs tests/NotificationSystem.Api.Tests
git commit -m "refactor: remove sync-over-async RabbitMQ startup paths"
```

---

### Task 5: Standardize Consumer Retry Configuration and Failure Logging

**Files:**
- Modify: `src/Consumers/NotificationSystem.Consumer.Email/Program.cs`
- Modify: `src/Consumers/NotificationSystem.Consumer.Sms/Program.cs`
- Modify: `src/Consumers/NotificationSystem.Consumer.Push/Program.cs`
- Modify: `src/Consumers/NotificationSystem.Consumer.Bulk/Program.cs`
- Modify: `src/NotificationSystem.Application/Consumers/MessageProcessingMiddleware.cs`
- Create: `tests/NotificationSystem.Api.Tests/Consumers/MessageProcessingMiddlewareLoggingTests.cs`

- [ ] **Step 1: Write failing assertion test for retry observability (log contract)**

```csharp
// tests/NotificationSystem.Api.Tests/Consumers/MessageProcessingMiddlewareLoggingTests.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Consumers;

namespace NotificationSystem.Api.Tests.Consumers;

public class MessageProcessingMiddlewareLoggingTests
{
    [Fact]
    public async Task Failure_Log_Should_Contain_ChannelType_And_Attempts()
    {
        var logger = new ListLogger<MessageProcessingMiddleware<string>>();
        var services = new ServiceCollection().BuildServiceProvider();
        var retryStrategy = new NoRetryStrategy();
        var middleware = new MessageProcessingMiddleware<string>(logger, services, retryStrategy);

        await middleware.ProcessWithErrorHandlingAsync(
            "payload",
            (_, _) => throw new InvalidOperationException("boom"),
            _ => Task.FromResult((Guid.NewGuid(), Guid.NewGuid())),
            typeof(string),
            CancellationToken.None);

        Assert.Contains(
            logger.Messages,
            message => message.Contains("ChannelType=String", StringComparison.Ordinal) &&
                       message.Contains("Attempts=1", StringComparison.Ordinal));
    }

    private sealed class NoRetryStrategy : IRetryStrategy
    {
        public bool ShouldRetry(int attemptNumber, Exception exception) => false;
        public TimeSpan GetRetryDelay(int attemptNumber) => TimeSpan.Zero;
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
```

- [ ] **Step 2: Replace hardcoded retries in workers with RabbitMqSettings.MaxRetryAttempts**

```csharp
// Example for each consumer Program.cs
builder.Services.AddSingleton<IRetryStrategy>(sp =>
{
    var rabbit = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
    return new ExponentialBackoffRetryStrategy(
        maxRetries: rabbit.MaxRetryAttempts,
        initialDelay: TimeSpan.FromSeconds(2),
        maxDelay: TimeSpan.FromMinutes(5));
});
```

- [ ] **Step 3: Enrich middleware failure logs for incident triage**

```csharp
// src/NotificationSystem.Application/Consumers/MessageProcessingMiddleware.cs
_logger.LogError(
    lastException,
    "Message processing failed after retries. ChannelType={ChannelType} Attempts={Attempts}",
    channelType.Name,
    attemptNumber + 1);
```

- [ ] **Step 4: Run build and focused tests**

Run:

```bash
dotnet build NotificationSystem.slnx -v minimal
```

Expected: `Build succeeded.`

Run:

```bash
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -v minimal
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Consumers src/NotificationSystem.Application/Consumers/MessageProcessingMiddleware.cs tests/NotificationSystem.Api.Tests
git commit -m "chore: centralize retry settings and improve failure observability"
```

---

### Task 6: Add CI Quality Gate and Operational Smoke Runbook

**Files:**
- Create: `.github/workflows/ci.yml`
- Create: `scripts/smoke/health-smoke.sh`
- Create: `docs/operations/reliability-baseline.md`
- Modify: `README.md`

- [ ] **Step 1: Write failing expectation by running CI commands locally**

Run:

```bash
dotnet restore NotificationSystem.slnx
dotnet build NotificationSystem.slnx -c Release --no-restore
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -c Release --no-build
```

Expected: all commands succeed.

- [ ] **Step 2: Create GitHub Actions workflow**

```yaml
# .github/workflows/ci.yml
name: ci

on:
  pull_request:
  push:
    branches: [ main ]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore NotificationSystem.slnx

      - name: Build
        run: dotnet build NotificationSystem.slnx -c Release --no-restore

      - name: Test
        run: dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -c Release --no-build -v minimal
```

- [ ] **Step 3: Add deterministic smoke script for health checks**

```bash
#!/usr/bin/env bash
set -euo pipefail

API_BASE_URL="${1:-http://localhost:5235}"

echo "Checking ${API_BASE_URL}/health/live"
curl --fail --silent "${API_BASE_URL}/health/live" | jq .status

echo "Checking ${API_BASE_URL}/health/ready"
curl --silent "${API_BASE_URL}/health/ready" | jq .status

echo "Health smoke completed"
```

- [ ] **Step 4: Document operational baseline and README entry**

```markdown
<!-- docs/operations/reliability-baseline.md -->
# Reliability Baseline

## Mandatory checks before release

1. `dotnet build NotificationSystem.slnx -c Release`
2. `dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -c Release`
3. `scripts/smoke/health-smoke.sh http://localhost:5235`

## Incident quick triage

1. Check `/health/live`
2. Check `/health/ready`
3. Inspect logs for `Message processing failed after retries`
4. Inspect DLQ endpoints (`/api/dlq/...`) and decide reprocess/purge path
```

```markdown
<!-- README.md (append section) -->
## Reliability Checks (0-30 days baseline)

- Health endpoints:
  - `GET /health/live`
  - `GET /health/ready`
- Run local smoke:
  - `./scripts/smoke/health-smoke.sh http://localhost:5235`
- CI gate:
  - Build + API reliability tests on PR/push
```

- [ ] **Step 5: Validate and commit**

Run:

```bash
chmod +x scripts/smoke/health-smoke.sh
dotnet restore NotificationSystem.slnx
dotnet build NotificationSystem.slnx -c Release --no-restore
dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -c Release --no-build -v minimal
```

Expected: all commands succeed.

Commit:

```bash
git add .github/workflows/ci.yml scripts/smoke/health-smoke.sh docs/operations/reliability-baseline.md README.md
git commit -m "ci: add reliability quality gate and operational smoke checks"
```

---

## Rollout Sequence (Recommended)

1. Task 1
2. Task 2
3. Task 3
4. Task 4
5. Task 5
6. Task 6

This order minimizes risk by validating app health visibility first, then hardening runtime behavior, then locking reliability controls into CI and runbooks.

## Verification Checklist (End of Plan)

- [ ] `dotnet build NotificationSystem.slnx -c Release` succeeds.
- [ ] `dotnet test tests/NotificationSystem.Api.Tests/NotificationSystem.Api.Tests.csproj -c Release` succeeds.
- [ ] `/health/live` responds 200 with JSON payload.
- [ ] `/health/ready` responds 200/503 with detailed checks.
- [ ] Non-whitelisted CORS origins are not allowed in non-dev environments.
- [ ] No sync-over-async startup calls remain in RabbitMQ publisher/DLQ service.
- [ ] CI workflow enforces build + tests on PR.
