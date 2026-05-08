using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.Login;

public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<Result<LoginCommandResponse>>;
