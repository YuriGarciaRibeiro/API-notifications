using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string? FullName,
    string? Email,
    bool? IsActive,
    List<Guid>? RoleIds) : IRequest<Result<UpdateUserResponse>>;
