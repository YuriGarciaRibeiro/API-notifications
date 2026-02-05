using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.UseCases.AssignRoles;
using NotificationSystem.Application.UseCases.ChangePassword;
using NotificationSystem.Application.UseCases.CreateUser;
using NotificationSystem.Application.UseCases.DeleteUser;
using NotificationSystem.Application.UseCases.GetAllUsers;
using NotificationSystem.Application.UseCases.GetUserById;
using NotificationSystem.Application.UseCases.UpdateUser;

namespace NotificationSystem.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Lista todos os usuários")
            .WithDescription(UserEndpointsDocumentation.GetAllUsersDescription)
            .RequireAuthorization("user.view")
            .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Obtém um usuário pelo ID")
            .WithDescription(UserEndpointsDocumentation.GetUserByIdDescription)
            .RequireAuthorization("user.view")
            .Produces<UserDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário")
            .WithDescription(UserEndpointsDocumentation.CreateUserDescription)
            .RequireAuthorization("user.create")
            .Produces<UserDto>(StatusCodes.Status201Created, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Atualiza um usuário")
            .WithDescription(UserEndpointsDocumentation.UpdateUserDescription)
            .RequireAuthorization("user.update")
            .Produces<UserDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Remove um usuário")
            .WithDescription(UserEndpointsDocumentation.DeleteUserDescription)
            .RequireAuthorization("user.delete")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("{id:guid}/change-password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Altera a senha do usuário")
            .WithDescription(UserEndpointsDocumentation.ChangePasswordDescription)
            .RequireAuthorization(Permissions.UserChangePassword)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("{id:guid}/assign-roles", AssignRoles)
            .WithName("AssignRoles")
            .WithSummary("Atribui roles a um usuário")
            .WithDescription(UserEndpointsDocumentation.AssignRolesDescription)
            .RequireAuthorization(Permissions.UserAssignRoles)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> GetAllUsers(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.FullName,
            request.RoleIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult(201);
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            id,
            request.FullName,
            request.Email,
            request.IsActive,
            request.RoleIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> ChangePassword(
        Guid id,
        [FromBody] ChangePasswordRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            id,
            request.CurrentPassword,
            request.NewPassword);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AssignRolesCommand(id, request.RoleIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }
}
