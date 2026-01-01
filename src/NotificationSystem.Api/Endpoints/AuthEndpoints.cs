using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("Login")
            .Produces<LoginResponse>(200)
            .Produces<ProblemDetails>(400);

        group.MapPost("/register", Register)
            .WithName("Register")
            .Produces<LoginResponse>(201)
            .Produces<ProblemDetails>(400);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .Produces<LoginResponse>(200)
            .Produces<ProblemDetails>(400);

        group.MapPost("/revoke", RevokeToken)
            .WithName("RevokeToken")
            .Produces(204)
            .Produces<ProblemDetails>(400);

        return group;
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IAuthenticationService authService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.LoginAsync(request, ipAddress);
        return result.ToIResult();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterUserRequest request,
        IAuthenticationService authService)
    {
        var result = await authService.RegisterAsync(request);
        return result.ToIResult(201);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        IAuthenticationService authService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.RefreshTokenAsync(request.RefreshToken, ipAddress);
        return result.ToIResult();
    }

    private static async Task<IResult> RevokeToken(
        [FromBody] RefreshTokenRequest request,
        IAuthenticationService authService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.RevokeTokenAsync(request.RefreshToken, ipAddress);
        return result.ToIResult();
    }
}
