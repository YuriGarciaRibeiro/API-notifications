using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.RevokeToken;

public record RevokeTokenCommand(string RefreshToken, string IpAddress) : IRequest<Result>;
