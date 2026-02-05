using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Application.UseCases.GetAuditLogById;
using NotificationSystem.Application.UseCases.GetAuditLogs;
using NotificationSystem.Application.UseCases.GetEntityAuditHistory;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Api.Endpoints;

/// <summary>
/// API endpoints for retrieving audit logs.
/// Provides read-only access to audit trail for compliance and troubleshooting.
/// </summary>
public static class AuditLogEndpoints
{
    /// <summary>
    /// Maps all audit log endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/audit-logs")
            .WithTags("Audit Logs")
            .RequireAuthorization(Permissions.AuditView);

        // GET /api/audit-logs - List with advanced filtering
        group.MapGet("/",
                async (
                    [FromQuery] string? entityName,
                    [FromQuery] string? entityId,
                    [FromQuery] Guid? userId,
                    [FromQuery] AuditAction? actionType,
                    [FromQuery] DateTime? startDate,
                    [FromQuery] DateTime? endDate,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20,
                    IMediator mediator = null!,
                    CancellationToken cancellationToken = default) =>
                {
                    var query = new GetAuditLogsQuery(
                        entityName,
                        entityId,
                        userId,
                        actionType,
                        startDate,
                        endDate,
                        pageNumber,
                        pageSize);

                    var result = await mediator.Send(query, cancellationToken);
                    return result.ToIResult();
                })
            .WithName("GetAuditLogs")
            .WithSummary("Lista todos os logs de auditoria com filtros opcionais")
            .WithDescription(
                @"Retorna logs de auditoria com suporte a filtros avançados:
- **entityName**: Filtrar por tipo de entidade (User, Notification, Role, etc.)
- **entityId**: Filtrar por ID específico da entidade
- **userId**: Filtrar por usuário que realizou a ação
- **actionType**: Filtrar por tipo de ação (Created, Updated, Deleted)
- **startDate**: Data inicial do período
- **endDate**: Data final do período
- **pageNumber**: Número da página (padrão: 1)
- **pageSize**: Tamanho da página (padrão: 20, máx: 100)

**Requer permissão**: `audit.view`")
            .Produces<GetAuditLogsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/audit-logs/{id} - Get specific audit log
        group.MapGet("/{id:guid}",
                async (
                    Guid id,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetAuditLogByIdQuery(id);
                    var result = await mediator.Send(query, cancellationToken);
                    return result.ToIResult();
                })
            .WithName("GetAuditLogById")
            .WithSummary("Obtém um log de auditoria específico pelo ID")
            .WithDescription(
                "Retorna os detalhes completos de um log de auditoria, incluindo valores antigos e novos.")
            .Produces<GetAuditLogByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/audit-logs/entity/{entityName}/{entityId} - Get entity history
        group.MapGet("/entity/{entityName}/{entityId}",
                async (
                    string entityName,
                    string entityId,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 20,
                    IMediator mediator = null!,
                    CancellationToken cancellationToken = default) =>
                {
                    var query = new GetEntityAuditHistoryQuery(
                        entityName,
                        entityId,
                        pageNumber,
                        pageSize);

                    var result = await mediator.Send(query, cancellationToken);
                    return result.ToIResult();
                })
            .WithName("GetEntityAuditHistory")
            .WithSummary("Obtém o histórico de auditoria de uma entidade específica")
            .WithDescription(
                @"Retorna todo o histórico de mudanças (criação, atualizações, exclusão) de uma entidade específica.

**Exemplos**:
- `/api/audit-logs/entity/User/550e8400-e29b-41d4-a716-446655440000`
- `/api/audit-logs/entity/Notification/123e4567-e89b-12d3-a456-426614174000`

**Útil para**: Visualizar a timeline completa de mudanças em um registro.")
            .Produces<GetEntityAuditHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
