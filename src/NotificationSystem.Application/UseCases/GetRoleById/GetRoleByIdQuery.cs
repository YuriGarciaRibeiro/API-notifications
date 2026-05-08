using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IRequest<Result<GetRoleByIdResponse>>;
