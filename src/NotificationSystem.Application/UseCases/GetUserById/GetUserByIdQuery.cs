using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.UseCases.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
