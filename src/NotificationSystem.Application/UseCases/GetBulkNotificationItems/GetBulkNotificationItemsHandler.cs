using FluentResults;
using MediatR;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetBulkNotificationItems;

public class GetBulkNotificationItemsHandler(IBulkNotificationRepository repository)
    : IRequestHandler<GetBulkNotificationItemsQuery, Result<IEnumerable<BulkNotificationItemResponse>>>
{
    private readonly IBulkNotificationRepository _repository = repository;

    public async Task<Result<IEnumerable<BulkNotificationItemResponse>>> Handle(
        GetBulkNotificationItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetItemsByJobIdAsync(
            request.JobId,
            request.Status,
            cancellationToken);

        if (!items.Any())
            //TODO mudar essa porra
            return Result.Fail(new NotFoundError("Notfound", request.JobId));

        var responses = items.Select(x => new BulkNotificationItemResponse(
            x.Id,
            x.Recipient,
            x.Channel.ToString(),
            x.Status.ToString(),
            x.ErrorMessage,
            x.SentAt,
            x.CreatedAt));

        return Result.Ok(responses);
    }
}
