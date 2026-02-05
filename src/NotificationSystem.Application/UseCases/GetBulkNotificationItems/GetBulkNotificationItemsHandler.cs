using FluentResults;
using MediatR;
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
            return Result.Fail(new Error("NotFound").WithMetadata("message", "No items found for this bulk job"));

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
