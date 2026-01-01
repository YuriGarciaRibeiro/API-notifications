using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Result>;
