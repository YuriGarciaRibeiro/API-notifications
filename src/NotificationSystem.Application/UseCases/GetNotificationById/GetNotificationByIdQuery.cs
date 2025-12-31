using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetNotificationById;

public record GetNotificationByIdQuery(Guid Id) : IRequest<Result<GetNotificationByIdResponse>>;
