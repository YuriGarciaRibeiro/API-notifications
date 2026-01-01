using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.UseCases.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string? FullName,
    string? Email,
    bool? IsActive,
    List<Guid>? RoleIds) : IRequest<Result<UserDto>>;
