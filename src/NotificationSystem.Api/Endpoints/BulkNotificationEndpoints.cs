using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.CreateBulkNotification;
using NotificationSystem.Application.UseCases.GetBulkNotificationJob;
using NotificationSystem.Application.UseCases.GetBulkNotificationProgress;
using NotificationSystem.Application.UseCases.GetBulkNotificationItems;
using NotificationSystem.Application.UseCases.CancelBulkNotification;
using NotificationSystem.Application.UseCases.GetAllBulkNotifications;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Api.Endpoints;

public static class BulkNotificationEndpoints
{
    public static IEndpointRouteBuilder MapBulkNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications/bulk")
            .WithTags("Bulk Notifications");

        // POST /api/notifications/bulk - Create bulk job
        group.MapPost("/",
            async ([FromBody] CreateBulkNotificationCommand command,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("CreateBulkNotification")
            .WithSummary("Cria um job de notificações em massa")
            .WithDescription("Cria um novo job para enviar notificações para múltiplos destinatários. " +
                           "Suporta até 1.000.000 de items por job. " +
                           "O processamento é assíncrono via RabbitMQ.")
            .RequireAuthorization(Permissions.BulkNotificationCreate)
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/notifications/bulk - List all bulk jobs with pagination
        group.MapGet("/",
            async (
                IMediator mediator,
                   CancellationToken cancellationToken, 
                   [FromQuery] int page = 1,
                   [FromQuery] int pageSize = 20,
                   [FromQuery] string? status = null,
                   [FromQuery] string sortBy = "createdAt",
                   [FromQuery] string sortOrder = "desc"
                   ) =>
            {
                var query = new GetAllBulkNotificationsQuery(page, pageSize, status, sortBy, sortOrder);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetAllBulkNotifications")
            .WithSummary("Lista todos os bulk jobs")
            .WithDescription("Retorna uma lista paginada de todos os jobs de notificações em massa. " +
                           "Suporta filtro por status e ordenação customizável.")
            .RequireAuthorization(Permissions.BulkNotificationView)
            .Produces<PagedBulkNotificationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // IMPORTANT: Register more specific routes BEFORE generic {jobId} routes
        // GET /api/notifications/bulk/{jobId}/progress - Get job progress
        group.MapGet("/{jobId:guid}/progress",
            async (Guid jobId,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var query = new GetBulkNotificationProgressQuery(jobId);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetBulkNotificationProgress")
            .WithSummary("Obtém progresso de um bulk job")
            .WithDescription("Monitora o progresso em tempo real de um job de notificações em massa. " +
                           "Útil para atualizar barras de progresso na UI.")
            .RequireAuthorization(Permissions.BulkNotificationView)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/notifications/bulk/{jobId}/items - Get job items
        group.MapGet("/{jobId:guid}/items",
            async (Guid jobId,
                   [FromQuery] NotificationStatus? status,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var query = new GetBulkNotificationItemsQuery(jobId, status);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetBulkNotificationItems")
            .WithSummary("Lista items de um bulk job")
            .WithDescription("Retorna todos os items de um bulk job, opcionalmente filtrados por status. " +
                           "Útil para listar items falhados ou verificar detalhes do envio.")
            .RequireAuthorization(Permissions.BulkNotificationView)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/notifications/bulk/{jobId} - Cancel bulk job
        group.MapDelete("/{jobId:guid}",
            async (Guid jobId,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var command = new CancelBulkNotificationCommand(jobId);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("CancelBulkNotificationJob")
            .WithSummary("Cancela um bulk job")
            .WithDescription("Cancela um job de notificações em massa que está aguardando ou em processamento. " +
                           "Jobs completados ou falhados não podem ser cancelados.")
            .RequireAuthorization(Permissions.BulkNotificationCancel)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/notifications/bulk/{jobId} - Get job details (MUST be AFTER specific routes)
        group.MapGet("/{jobId:guid}",
            async (Guid jobId,
                   IMediator mediator,
                   CancellationToken cancellationToken) =>
            {
                var query = new GetBulkNotificationJobQuery(jobId);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetBulkNotificationJob")
            .WithSummary("Obtém detalhes de um bulk job")
            .WithDescription("Retorna informações completas de um job de notificações em massa, " +
                           "incluindo status, progresso, contagem de sucessos e falhas.")
            .RequireAuthorization(Permissions.BulkNotificationView)
            .Produces<BulkNotificationJobDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
