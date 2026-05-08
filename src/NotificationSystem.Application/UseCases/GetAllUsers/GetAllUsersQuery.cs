using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<GetAllUsersResponse>>;
