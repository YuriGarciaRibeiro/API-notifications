using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.CreateProvider;
using NotificationSystem.Application.UseCases.CreateProviderFromFile;
using NotificationSystem.Application.UseCases.GetAllProviders;
using NotificationSystem.Application.UseCases.SetProviderAsPrimary;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Api.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/providers")
            .WithTags("Provider Configuration")
            .RequireAuthorization()
            .DisableAntiforgery(); // Necessário para upload de arquivos

        group.MapGet("/",
            async ([FromQuery] ChannelType? channelType, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetAllProvidersQuery(channelType);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetAllProviders")
            .WithSummary("Lista todos os provedores configurados")
            .WithDescription("Retorna a lista de provedores de notificação configurados, opcionalmente filtrados por tipo de canal.")
            .Produces<List<ProviderConfigurationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/",
            async ([FromBody] CreateProviderCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created($"/api/admin/providers/{result.Value}", new { id = result.Value });
                }

                return result.ToIResult();
            })
            .WithName("CreateProvider")
            .WithSummary("Cadastra um novo provedor")
            .WithDescription("Cria uma nova configuração de provedor de notificação (Twilio, SMTP, Firebase, etc).")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/upload",
            async (
                [FromForm] IFormFile file,
                [FromForm] ChannelType channelType,
                [FromForm] ProviderType provider,
                [FromForm] string? projectId,
                [FromForm] bool isActive,
                [FromForm] bool isPrimary,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                // Validação básica do arquivo
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "File is required" });
                }

                // Abre o stream do arquivo
                var stream = file.OpenReadStream();

                var command = new CreateProviderFromFileCommand(
                    channelType,
                    provider,
                    stream,
                    file.FileName,
                    file.Length,
                    projectId,
                    isActive,
                    isPrimary
                );

                var result = await mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created($"/api/admin/providers/{result.Value}", new { id = result.Value });
                }

                return result.ToIResult();
            })
            .WithName("CreateProviderFromFile")
            .WithSummary("Cadastra um novo provedor via upload de arquivo")
            .WithDescription("Faz upload do arquivo de credenciais (ex: Firebase JSON) e cria uma nova configuração de provedor.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{id:guid}/set-primary",
            async (Guid id, [FromQuery] ChannelType channelType, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new SetProviderAsPrimaryCommand(id, channelType);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("SetProviderAsPrimary")
            .WithSummary("Define um provedor como primário")
            .WithDescription("Ativa um provedor como primário para o canal especificado, desativando os outros automaticamente.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
