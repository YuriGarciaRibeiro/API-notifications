using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<GetAllRolesResponse>>;
