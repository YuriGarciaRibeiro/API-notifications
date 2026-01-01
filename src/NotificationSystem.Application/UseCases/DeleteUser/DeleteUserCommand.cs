using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;
