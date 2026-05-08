using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Application.Authorization;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Api.Endpoints.Requests;
using NotificationSystem.Application.UseCases.CreateProvider;
using NotificationSystem.Application.UseCases.CreateProviderFromUpload;
using NotificationSystem.Application.UseCases.DeleteProvider;
using NotificationSystem.Application.UseCases.GetAllProviders;
using NotificationSystem.Application.UseCases.GetProviderMetadata;
using NotificationSystem.Application.UseCases.GetProviderConfiguration;
using NotificationSystem.Application.UseCases.SetProviderAsPrimary;
using NotificationSystem.Application.UseCases.TestProviderConnection;
using NotificationSystem.Application.UseCases.ToggleProviderActive;
using NotificationSystem.Application.UseCases.UpdateProviderConfiguration;
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
            .Produces<GetAllProvidersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/metadata",
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetProviderMetadataQuery();
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetProviderMetadata")
            .WithSummary("Retorna metadata de formulário para providers")
            .WithDescription("Retorna catálogo estático de canais, provedores e campos para montagem dinâmica do formulário no frontend.")
            .RequireAuthorization(Permissions.ProviderView)
            .Produces<ProviderMetadataResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}/configuration",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetProviderConfigurationQuery(id);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("GetProviderConfiguration")
            .WithSummary("Obtém configuração segura de um provedor")
            .WithDescription("Retorna configuração não sensível de um provedor com flags indicando se segredos estão configurados.")
            .RequireAuthorization(Permissions.ProviderView)
            .Produces<ProviderConfigurationDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/",
            async ([FromBody] CreateProviderCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Created($"/api/admin/providers/{result.Value.ProviderId}", result.Value);
                }

                return result.ToIResult();
            })
            .WithName("CreateProvider")
            .WithSummary("Cadastra um novo provedor")
            .WithDescription("Cria uma nova configuração de provedor de notificação (Twilio, SMTP, Firebase, etc).")
            .RequireAuthorization(Permissions.ProviderCreate)
            .Produces<CreateProviderResponse>(StatusCodes.Status201Created)
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
                    return Results.Created($"/api/admin/providers/{result.Value.ProviderId}", result.Value);
                }

                return result.ToIResult();
            })
            .WithName("CreateProviderFromFile")
            .WithSummary("Cadastra um novo provedor via upload de arquivo")
            .WithDescription("Faz upload do arquivo de credenciais (ex: Firebase JSON) e cria uma nova configuração de provedor.")
            .RequireAuthorization(Permissions.ProviderUpload)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<CreateProviderFromUploadResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}",
            async (
                Guid id,
                [FromBody] UpdateProviderConfigurationRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateProviderConfigurationCommand(id, request.Configuration);
                var result = await mediator.Send(command, cancellationToken);
                return result.ToIResult();
            })
            .WithName("UpdateProviderConfiguration")
            .WithSummary("Atualiza a configuração de um provedor")
            .WithDescription("Atualiza parcialmente a configuração de um provedor sem expor segredos em responses.")
            .RequireAuthorization(Permissions.ProviderUpdate)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{id:guid}/test-connection",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new TestProviderConnectionQuery(id);
                var result = await mediator.Send(query, cancellationToken);
                return result.ToIResult();
            })
            .WithName("TestProviderConnection")
            .WithSummary("Valida conexão de um provedor sem envio real")
            .WithDescription("Executa smoke test de conectividade/credenciais por tipo de provider sem disparar notificação real.")
            .RequireAuthorization(Permissions.ProviderUpdate)
            .Produces<TestProviderConnectionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
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
