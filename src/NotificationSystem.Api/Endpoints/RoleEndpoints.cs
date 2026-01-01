using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Api.Endpoints;

public static class RoleEndpoints
{
    public static RouteGroupBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles");

        group.MapGet("", GetAllRoles)
            .WithName("GetAllRoles")
            .RequireAuthorization("role.view")
            .Produces<IEnumerable<RoleDetailDto>>(200);

        group.MapGet("{id:guid}", GetRoleById)
            .WithName("GetRoleById")
            .RequireAuthorization("role.view")
            .Produces<RoleDetailDto>(200)
            .Produces<ProblemDetails>(404);

        group.MapGet("permissions", GetAllPermissions)
            .WithName("GetAllPermissions")
            .RequireAuthorization()
            .Produces<IEnumerable<PermissionDto>>(200);

        group.MapPost("", CreateRole)
            .WithName("CreateRole")
            .RequireAuthorization("role.create")
            .Produces<RoleDetailDto>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(403);

        group.MapPut("{id:guid}", UpdateRole)
            .WithName("UpdateRole")
            .RequireAuthorization("role.update")
            .Produces<RoleDetailDto>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        group.MapDelete("{id:guid}", DeleteRole)
            .WithName("DeleteRole")
            .RequireAuthorization("role.delete")
            .Produces(204)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        return group;
    }

    private static async Task<IResult> GetAllRoles(IRoleManagementService roleService)
    {
        var result = await roleService.GetAllAsync();
        return result.ToIResult();
    }

    private static async Task<IResult> GetRoleById(Guid id, IRoleManagementService roleService)
    {
        var result = await roleService.GetByIdAsync(id);
        return result.ToIResult();
    }

    private static async Task<IResult> GetAllPermissions(IRoleManagementService roleService)
    {
        var result = await roleService.GetAllPermissionsAsync();
        return result.ToIResult();
    }

    private static async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        IRoleManagementService roleService)
    {
        var result = await roleService.CreateAsync(request);
        return result.ToIResult(201);
    }

    private static async Task<IResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        IRoleManagementService roleService)
    {
        var result = await roleService.UpdateAsync(id, request);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteRole(Guid id, IRoleManagementService roleService)
    {
        var result = await roleService.DeleteAsync(id);
        return result.ToIResult();
    }
}
