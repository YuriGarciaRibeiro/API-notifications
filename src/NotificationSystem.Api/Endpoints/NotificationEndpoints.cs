using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.GetAllNotifications;

namespace NotificationSystem.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapGet("/",
        async ([AsParameters] GetAllNotificationsRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAllNotificationsQuery(request.PageNumber, request.PageSize);
            var result = await mediator.Send(query, cancellationToken);

            return result.ToIResult();
        })
            .WithName("GetAllNotifications")
            .WithSummary("Lista todas as notificações")
            .WithDescription(@"
Retorna uma lista paginada de todas as notificações do sistema.

As notificações são retornadas de forma polimórfica, onde cada tipo (Email, SMS, Push)
possui seus campos específicos além dos campos comuns.

**Tipos de Notificação:**
- **Email**: Contém subject, body, to, isBodyHtml
- **SMS**: Contém message, to, senderId
- **Push**: Contém content (title, body), to, data, priority, isRead

**Campos Comuns:**
- id, userId, createdAt, status, errorMessage, sentAt
")
            .Produces<GetAllNotificationsResponse>(StatusCodes.Status200OK, "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}

public record GetAllNotificationsRequest(
    int PageNumber = 1,
    int PageSize = 10
);