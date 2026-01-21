using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.DTOs.DeadLetter;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Api.Endpoints;

public static class DeadLetterQueueEndpoints
{
    private static readonly Dictionary<string, string> QueueMapping = new()
    {
        { "sms-notifications-dlq", "sms-notifications" },
        { "email-notifications-dlq", "email-notifications" },
        { "push-notifications-dlq", "push-notifications" }
    };

    public static IEndpointRouteBuilder MapDeadLetterQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dlq")
            .WithTags("Dead Letter Queue")
            .RequireAuthorization();

        group.MapGet("/stats",
            async (IDeadLetterQueueService dlqService) =>
            {
                var stats = await dlqService.GetAllDeadLetterQueueStatsAsync();
                return Results.Ok(stats);
            })
            .WithName("GetDLQStats")
            .WithSummary("Obtém estatísticas de todas as Dead Letter Queues")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.GetStatsDescription)
            .Produces<IEnumerable<DeadLetterQueueStatsDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{queueName}/messages",
            async (
                string queueName,
                IDeadLetterQueueService dlqService,
                [FromQuery] int limit = 100) =>
            {
                if (!QueueMapping.ContainsKey(queueName))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid queue name",
                        validQueues = QueueMapping.Keys
                    });
                }

                var messages = await dlqService.GetDeadLetterMessagesAsync(queueName, limit);
                return Results.Ok(messages);
            })
            .WithName("GetDLQMessages")
            .WithSummary("Lista as mensagens de uma Dead Letter Queue específica")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.GetMessagesDescription)
            .Produces<IEnumerable<DeadLetterMessageDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{queueName}/reprocess/{deliveryTag}",
            async (
                string queueName,
                ulong deliveryTag,
                IDeadLetterQueueService dlqService) =>
            {
                if (!QueueMapping.TryGetValue(queueName, out var originalQueue))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid queue name",
                        validQueues = QueueMapping.Keys
                    });
                }

                await dlqService.ReprocessMessageAsync(queueName, originalQueue, deliveryTag);

                return Results.Ok(new
                {
                    message = "Message reprocessed successfully",
                    dlqName = queueName,
                    originalQueue,
                    deliveryTag
                });
            })
            .WithName("ReprocessDLQMessage")
            .WithSummary("Reprocessa uma mensagem específica da DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.ReprocessMessageDescription)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{queueName}/reprocess-all",
            async (
                string queueName,
                IDeadLetterQueueService dlqService) =>
            {
                if (!QueueMapping.TryGetValue(queueName, out var originalQueue))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid queue name",
                        validQueues = QueueMapping.Keys
                    });
                }

                await dlqService.ReprocessAllMessagesAsync(queueName, originalQueue);

                return Results.Ok(new
                {
                    message = "All messages reprocessed successfully",
                    dlqName = queueName,
                    originalQueue
                });
            })
            .WithName("ReprocessAllDLQMessages")
            .WithSummary("Reprocessa todas as mensagens de uma DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.ReprocessAllDescription)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{queueName}/purge",
            async (
                string queueName,
                IDeadLetterQueueService dlqService) =>
            {
                if (!QueueMapping.ContainsKey(queueName))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid queue name",
                        validQueues = QueueMapping.Keys
                    });
                }

                await dlqService.PurgeDeadLetterQueueAsync(queueName);

                return Results.Ok(new
                {
                    message = "Queue purged successfully",
                    queueName
                });
            })
            .WithName("PurgeDLQ")
            .WithSummary("Remove todas as mensagens de uma DLQ")
            .WithDescription(DeadLetterQueueEndpointsDocumentation.PurgeDescription)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
