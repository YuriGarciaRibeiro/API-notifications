using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.GetDLQMessages;
using NotificationSystem.Application.UseCases.GetDLQStats;
using NotificationSystem.Application.UseCases.PurgeDLQ;
using NotificationSystem.Application.UseCases.ReprocessAllDLQMessages;
using NotificationSystem.Application.UseCases.ReprocessDLQMessage;

namespace NotificationSystem.Api.Endpoints;

public static class DeadLetterQueueEndpoints
{
    public static IEndpointRouteBuilder MapDeadLetterQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dlq")
            .WithTags("Dead Letter Queue")
            .RequireAuthorization();

        group.MapGet("/stats",
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetDLQStatsQuery(), cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetDLQStats")
            .WithSummary("Obtém estatísticas de todas as Dead Letter Queues")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.GetStatsDescription)
            .RequireAuthorization(Permissions.DlqView)
            .Produces<GetDLQStatsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{queueName}/messages",
            async (
                string queueName,
                IMediator mediator,
                CancellationToken cancellationToken,
                [FromQuery] int limit = 100) =>
            {
                var result = await mediator.Send(new GetDLQMessagesQuery(queueName, limit), cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetDLQMessages")
            .WithSummary("Lista as mensagens de uma Dead Letter Queue específica")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.GetMessagesDescription)
            .RequireAuthorization(Permissions.DlqView)
            .Produces<GetDLQMessagesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{queueName}/reprocess/{deliveryTag}",
            async (
                string queueName,
                ulong deliveryTag,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new ReprocessDLQMessageCommand(queueName, deliveryTag);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("ReprocessDLQMessage")
            .WithSummary("Reprocessa uma mensagem específica da DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.ReprocessMessageDescription)
            .RequireAuthorization(Permissions.DlqReprocess)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{queueName}/reprocess-all",
            async (
                string queueName,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new ReprocessAllDLQMessagesCommand(queueName);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("ReprocessAllDLQMessages")
            .WithSummary("Reprocessa todas as mensagens de uma DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.ReprocessAllDescription)
            .RequireAuthorization(Permissions.DlqReprocess)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{queueName}/purge",
            async (
                string queueName,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new PurgeDLQCommand(queueName);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("PurgeDLQ")
            .WithSummary("Remove todas as mensagens de uma DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.PurgeDescription)
            .RequireAuthorization(Permissions.DlqPurge)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
