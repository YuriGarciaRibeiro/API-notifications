using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<Result<RefreshTokenCommandResponse>>;
