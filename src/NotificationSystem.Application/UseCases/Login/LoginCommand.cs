using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.Auth;

namespace NotificationSystem.Application.UseCases.Login;

public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<Result<LoginResponse>>;
