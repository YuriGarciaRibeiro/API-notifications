using NotificationSystem.Api.Endpoints;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Middlewares;
using NotificationSystem.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddCustomProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure ResultExtensions
ResultExtensions.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
app.UseSwaggerConfiguration(app.Environment);

app.UseHttpsRedirection();

// Map endpoints
app.MapNotificationEndpoints();

app.Run();
