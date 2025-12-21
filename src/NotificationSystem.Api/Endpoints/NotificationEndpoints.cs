using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.CreateNotification;
using NotificationSystem.Application.UseCases.GetAllNotifications;

namespace NotificationSystem.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapGet("/",
            async ([AsParameters] PaginationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetAllNotificationsQuery(request.PageNumber, request.PageSize);
                var result = await mediator.Send(query, cancellationToken);

                return result.ToIResult();
             })
            .WithName("GetAllNotifications")
            .WithSummary("Lista todas as notificações")
            .WithDescription(NotificationEndpointsDocumentation.GetAllNotificationsDescription)
            .Produces<GetAllNotificationsResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/",
            async ([FromBody] CreateNotificationCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("CreateNotification")
            .WithSummary("Cria uma nova notificação multi-canal")
            .WithDescription(NotificationEndpointsDocumentation.CreateNotificationDescription)
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}

public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 10
);