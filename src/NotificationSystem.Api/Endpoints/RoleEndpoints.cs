using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.DTOs.Common;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.UseCases.CreateRole;
using NotificationSystem.Application.UseCases.DeleteRole;
using NotificationSystem.Application.UseCases.GetAllPermissions;
using NotificationSystem.Application.UseCases.GetAllRoles;
using NotificationSystem.Application.UseCases.GetRoleById;
using NotificationSystem.Application.UseCases.UpdateRole;

namespace NotificationSystem.Api.Endpoints;

public static class RoleEndpoints
{
    public static RouteGroupBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles");

        group.MapGet("", GetAllRoles)
            .WithName("GetAllRoles")
            .WithSummary("Lista todas as roles")
            .WithDescription(RoleEndpointsDocumentation.GetAllRolesDescription)
            .RequireAuthorization("role.view")
            .Produces<IEnumerable<RoleDetailDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("{id:guid}", GetRoleById)
            .WithName("GetRoleById")
            .WithSummary("Obtém uma role pelo ID")
            .WithDescription(RoleEndpointsDocumentation.GetRoleByIdDescription)
            .RequireAuthorization("role.view")
            .Produces<RoleDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("permissions", GetAllPermissions)
            .WithName("GetAllPermissions")
            .WithSummary("Lista todas as permissões")
            .WithDescription(RoleEndpointsDocumentation.GetAllPermissionsDescription)
            .RequireAuthorization()
            .Produces<IEnumerable<PermissionDto>>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("", CreateRole)
            .WithName("CreateRole")
            .WithSummary("Cria uma nova role")
            .WithDescription(RoleEndpointsDocumentation.CreateRoleDescription)
            .RequireAuthorization("role.create")
            .Produces<RoleDetailDto>(StatusCodes.Status201Created, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("{id:guid}", UpdateRole)
            .WithName("UpdateRole")
            .WithSummary("Atualiza uma role")
            .WithDescription(RoleEndpointsDocumentation.UpdateRoleDescription)
            .RequireAuthorization("role.update")
            .Produces<RoleDetailDto>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("{id:guid}", DeleteRole)
            .WithName("DeleteRole")
            .WithSummary("Remove uma role")
            .WithDescription(RoleEndpointsDocumentation.DeleteRoleDescription)
            .RequireAuthorization("role.delete")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> GetAllRoles(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllRolesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> GetRoleById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> GetAllPermissions(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllPermissionsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand(
            request.Name,
            request.Description,
            request.PermissionIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult(201);
    }

    private static async Task<IResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand(
            id,
            request.Name,
            request.Description,
            request.PermissionIds);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteRole(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToIResult();
    }
}
