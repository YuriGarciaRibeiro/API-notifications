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

**Arquitetura Multi-Canal:**
Cada notificação pode ter um ou mais canais de entrega (Email, SMS, Push).
Os canais são retornados de forma polimórfica, onde cada tipo possui seus campos específicos.

**Estrutura da Notificação:**
- **id**: ID único da notificação
- **userId**: ID do usuário que receberá a notificação
- **createdAt**: Data/hora de criação
- **channels**: Lista de canais de entrega (pode conter múltiplos canais)

**Tipos de Canal:**
- **Email**: subject, body, to, isBodyHtml
- **SMS**: message, to, senderId
- **Push**: content (title, body, clickAction), to, data, priority, isRead

**Campos Comuns dos Canais:**
- id, status, errorMessage, sentAt

**Exemplos de Uso:**
- Notificação apenas por Email: 1 canal Email
- Lembrete de consulta: 2 canais (Email + SMS)
- Alerta de segurança: 3 canais (Email + SMS + Push)
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