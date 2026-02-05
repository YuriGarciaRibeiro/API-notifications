using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;

namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public class CreateBulkNotificationHandler(IBulkNotificationRepository repository, IMessagePublisher messagePublisher, ILogger<CreateBulkNotificationHandler> logger, ICurrentUserService currentUserService) : IRequestHandler<CreateBulkNotificationCommand, Result<Guid>>
{
    private readonly IBulkNotificationRepository _repository = repository;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly ILogger<CreateBulkNotificationHandler> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Guid>> Handle(CreateBulkNotificationCommand request, CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid();
        var job = new BulkNotificationJob
        {
            Id = jobId,
            Name = request.Name,
            Description = request.Description,
            CreatedBy = _currentUserService.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            TotalCount = request.Items.Count,
            Items = request.Items.Select(i => new BulkNotificationItem
            {
                Id = Guid.NewGuid(),
                BulkJobId = jobId,
                Recipient = i.Recipient,
                Channel = i.Channel,
                Variables = i.Variables ?? new(),
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _repository.CreateJobAsync(job, cancellationToken);
        _logger.LogInformation("Bulk notification job {JobId} created with {ItemCount} items.", jobId, job.Items.Count);

        // Publica evento para processamento assíncrono
        await _messagePublisher.PublishAsync("bulk-notifications", new BulkNotificationJobMessage(jobId), cancellationToken);
        _logger.LogInformation("Bulk notification job {JobId} published for processing.", jobId);

        return Result.Ok(jobId);
    }
}