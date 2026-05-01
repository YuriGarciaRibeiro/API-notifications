using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Endpoints.Requests;
using NotificationSystem.Application.UseCases.CreateProvider;
using NotificationSystem.Application.UseCases.CreateProviderFromUpload;
using NotificationSystem.Application.UseCases.DeleteProvider;
using NotificationSystem.Application.UseCases.GetAllProviders;
using NotificationSystem.Application.UseCases.SetProviderAsPrimary;
using NotificationSystem.Application.UseCases.ToggleProviderActive;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Api.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/providers")
            .WithTags("Provider Configuration")
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
            .RequireAuthorization(Permissions.ProviderView)
            .Produces<List<ProviderConfigurationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
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
            .RequireAuthorization(Permissions.ProviderCreate)
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/upload",
            async (UploadProviderFormRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new CreateProviderFromUploadCommand(
                    request.ChannelType,
                    request.Provider,
                    request.File?.OpenReadStream(),
                    request.File?.FileName,
                    request.File?.Length ?? 0,
                    request.ProjectId,
                    request.IsActive,
                    request.IsPrimary
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
            .RequireAuthorization(Permissions.ProviderUpload)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{id:guid}/set-primary",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new SetProviderAsPrimaryCommand(id);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("SetProviderAsPrimary")
            .WithSummary("Define um provedor como primário")
            .WithDescription("Ativa um provedor como primário para o canal, desativando os outros do mesmo canal automaticamente.")
            .RequireAuthorization(Permissions.ProviderSetPrimary)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{id:guid}/toggle-active",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new ToggleProviderActiveCommand(id);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("ToggleProviderActiveStatus")
            .WithSummary("Ativa ou desativa um provedor")
            .WithDescription("Alterna o status ativo/inativo de um provedor de notificação.")
            .RequireAuthorization(Permissions.ProviderToggle)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new DeleteProviderCommand(id);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("DeleteProvider")
            .WithSummary("Remove um provedor")
            .WithDescription("Exclui uma configuração de provedor de notificação do sistema.")
            .RequireAuthorization(Permissions.ProviderDelete)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
