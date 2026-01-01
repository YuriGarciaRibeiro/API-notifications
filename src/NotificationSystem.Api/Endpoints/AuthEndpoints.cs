using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Auth;
using NotificationSystem.Application.UseCases.Login;
using NotificationSystem.Application.UseCases.RefreshToken;
using NotificationSystem.Application.UseCases.Register;
using NotificationSystem.Application.UseCases.RevokeToken;

namespace NotificationSystem.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Autentica um usuário no sistema")
            .WithDescription(AuthEndpointsDocumentation.LoginDescription)
            .Produces<LoginResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Registra um novo usuário")
            .WithDescription(AuthEndpointsDocumentation.RegisterDescription)
            .Produces<LoginResponse>(StatusCodes.Status201Created, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Renova o access token")
            .WithDescription(AuthEndpointsDocumentation.RefreshTokenDescription)
            .Produces<LoginResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/revoke", RevokeToken)
            .WithName("RevokeToken")
            .WithSummary("Revoga um refresh token (logout)")
            .WithDescription(AuthEndpointsDocumentation.RevokeTokenDescription)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new LoginCommand(request.Email, request.Password, ipAddress);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FullName, request.RoleIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult(201);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> RevokeToken(
        [FromBody] RefreshTokenRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RevokeTokenCommand(request.RefreshToken, ipAddress);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }
}
