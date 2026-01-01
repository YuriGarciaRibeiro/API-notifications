using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("", GetAllUsers)
            .WithName("GetAllUsers")
            .RequireAuthorization("user.view")
            .Produces<IEnumerable<UserDto>>(200);

        group.MapGet("{id:guid}", GetUserById)
            .WithName("GetUserById")
            .RequireAuthorization("user.view")
            .Produces<UserDto>(200)
            .Produces<ProblemDetails>(404);

        group.MapPost("", CreateUser)
            .WithName("CreateUser")
            .RequireAuthorization("user.create")
            .Produces<UserDto>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(403);

        group.MapPut("{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .RequireAuthorization("user.update")
            .Produces<UserDto>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        group.MapDelete("{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .RequireAuthorization("user.delete")
            .Produces(204)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        group.MapPost("{id:guid}/change-password", ChangePassword)
            .WithName("ChangePassword")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(400);

        group.MapPost("{id:guid}/assign-roles", AssignRoles)
            .WithName("AssignRoles")
            .RequireAuthorization("user.update")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(403);

        return group;
    }

    private static async Task<IResult> GetAllUsers(IUserManagementService userService)
    {
        var result = await userService.GetAllAsync();
        return result.ToIResult();
    }

    private static async Task<IResult> GetUserById(Guid id, IUserManagementService userService)
    {
        var result = await userService.GetByIdAsync(id);
        return result.ToIResult();
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        IUserManagementService userService)
    {
        var result = await userService.CreateAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.RoleIds);
        return result.ToIResult(201);
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        IUserManagementService userService)
    {
        var result = await userService.UpdateAsync(id, request);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteUser(Guid id, IUserManagementService userService)
    {
        var result = await userService.DeleteAsync(id);
        return result.ToIResult();
    }

    private static async Task<IResult> ChangePassword(
        Guid id,
        [FromBody] ChangePasswordRequest request,
        IUserManagementService userService)
    {
        var result = await userService.ChangePasswordAsync(id, request);
        return result.ToIResult();
    }

    private static async Task<IResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesRequest request,
        IUserManagementService userService)
    {
        var result = await userService.AssignRolesAsync(id, request.RoleIds);
        return result.ToIResult();
    }
}

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds);

public record AssignRolesRequest(List<Guid> RoleIds);
