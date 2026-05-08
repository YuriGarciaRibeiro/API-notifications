using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<GetUserByIdResponse>>;
