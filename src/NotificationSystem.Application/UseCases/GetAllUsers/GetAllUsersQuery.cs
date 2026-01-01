using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.UseCases.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<IEnumerable<UserDto>>>;
