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
        var job = await _repository.GetWithItemsAsync(request.JobId, cancellationToken);
        if (job is null)
            return Result.Fail(new NotFoundError("Bulk notification job", request.JobId));

        var items = await _repository.GetItemsByJobIdAsync(
            request.JobId,
            request.Status,
            cancellationToken);

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
