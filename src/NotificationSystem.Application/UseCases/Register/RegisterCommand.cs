using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Auth;

namespace NotificationSystem.Application.UseCases.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    List<Guid> RoleIds) : IRequest<Result<LoginResponse>>;
