using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Api.Extensions;
using NotificationSystem.Application.UseCases.CreateProvider;
using NotificationSystem.Application.UseCases.CreateProviderFromFile;
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
            async (HttpRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                // Verifica se tem arquivo
                if (!request.HasFormContentType || request.Form.Files.Count == 0)
                {
                    return Results.BadRequest(new { error = "File is required" });
                }

                var file = request.Form.Files["file"];
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { error = "File is required and cannot be empty" });
                }

                // Extrai os parâmetros do form
                var channelTypeStr = request.Form["channelType"].ToString();
                var providerStr = request.Form["provider"].ToString();
                var projectId = request.Form["projectId"].ToString();
                var isActiveStr = request.Form["isActive"].ToString();
                var isPrimaryStr = request.Form["isPrimary"].ToString();

                // Valida e converte os parâmetros
                if (!Enum.TryParse<ChannelType>(channelTypeStr, out var channelType))
                {
                    return Results.BadRequest(new { error = "Invalid channelType" });
                }

                if (!Enum.TryParse<ProviderType>(providerStr, out var provider))
                {
                    return Results.BadRequest(new { error = "Invalid provider" });
                }

                var isActive = string.IsNullOrEmpty(isActiveStr) || bool.Parse(isActiveStr);
                var isPrimary = !string.IsNullOrEmpty(isPrimaryStr) && bool.Parse(isPrimaryStr);

                // Abre o stream do arquivo
                var stream = file.OpenReadStream();

                var command = new CreateProviderFromFileCommand(
                    channelType,
                    provider,
                    stream,
                    file.FileName,
                    file.Length,
                    string.IsNullOrEmpty(projectId) ? null : projectId,
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
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/{id:guid}/toggle-active",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new ToggleProviderActiveCommand(id);
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("ToggleProviderActiveStatus")
            .WithSummary("Ativa ou desativa um provedor")
            .WithDescription("Alterna o status ativo/inativo de um provedor de notificação.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var command = new DeleteProviderCommand(id);
                await mediator.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteProvider")
            .WithSummary("Remove um provedor")
            .WithDescription("Exclui uma configuração de provedor de notificação do sistema.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
