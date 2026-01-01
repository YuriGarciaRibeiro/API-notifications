using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Auth;

namespace NotificationSystem.Application.UseCases.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<Result<LoginResponse>>;
