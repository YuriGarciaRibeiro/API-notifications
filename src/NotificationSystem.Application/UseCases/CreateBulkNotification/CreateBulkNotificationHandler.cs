using System.Diagnostics;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;
using NotificationSystem.Application.Services;

namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public class CreateBulkNotificationHandler(
    IBulkNotificationRepository repository,
    IMessagePublisher messagePublisher,
    ICampaignSchedulerService campaignScheduler,
    ILogger<CreateBulkNotificationHandler> logger,
    ICurrentUserService currentUserService) : IRequestHandler<CreateBulkNotificationCommand, Result<Guid>>
{
    private readonly IBulkNotificationRepository _repository = repository;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly ICampaignSchedulerService _campaignScheduler = campaignScheduler;
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
            ScheduledFor = request.ScheduledFor,
            RecurringCron = request.RecurringCron,
            TimeZone = request.TimeZone ?? "UTC",
            Items = [.. request.Items.Select(i => new BulkNotificationItem
            {
                Id = Guid.NewGuid(),
                BulkJobId = jobId,
                Recipient = i.Recipient,
                Channel = i.Channel,
                Variables = i.Variables ?? new(),
                Status = request.RecurringCron != null
                    ? NotificationStatus.Scheduled
                    :request.ScheduledFor.HasValue
                        ? NotificationStatus.Scheduled
                        : NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            })]
        };

        // Determinar status inicial
        if (job.IsRecurring)
        {
            job.Status = BulkJobStatus.Scheduled;
            _logger.LogInformation("Bulk notification job {JobId} will be recurring with cron: {Cron}", jobId, request.RecurringCron);
        }
        else if (job.ScheduledFor.HasValue)
        {
            job.Status = BulkJobStatus.Scheduled;
            _logger.LogInformation("Bulk notification job {JobId} scheduled for {ScheduledFor}", jobId, request.ScheduledFor);
        }
        else
        {
            job.Status = BulkJobStatus.Pending;
            _logger.LogInformation("Bulk notification job {JobId} will be processed immediately", jobId);
        }

        await _repository.CreateJobAsync(job, cancellationToken);
        _logger.LogInformation("Bulk notification job {JobId} created with {ItemCount} items.", jobId, job.Items.Count);

        // Agendar ou publicar imediatamente
        if (job.IsRecurring && !string.IsNullOrEmpty(request.RecurringCron))
        {
            // Campanha recorrente
            _campaignScheduler.ScheduleRecurringCampaign(jobId, request.RecurringCron, request.TimeZone);
            _logger.LogInformation("Recurring campaign {JobId} scheduled with cron: {Cron}", jobId, request.RecurringCron);
        }
        else if (job.ScheduledFor.HasValue)
        {
            // Campanha agendada única
            var hangfireJobId = _campaignScheduler.ScheduleCampaign(jobId, job.ScheduledFor.Value);
            job.HangfireJobId = hangfireJobId;
            await _repository.UpdateJobAsync(job, cancellationToken);
            _logger.LogInformation("Campaign {JobId} scheduled for {ScheduledFor}. Hangfire job: {HangfireJobId}",
                jobId, job.ScheduledFor, hangfireJobId);
        }
        else
        {
            // Processar imediatamente
            await _messagePublisher.PublishAsync("bulk-notifications", new BulkNotificationJobMessage(jobId), cancellationToken);
            _logger.LogInformation("Bulk notification job {JobId} published for immediate processing.", jobId);
        }

        return Result.Ok(jobId);
    }
}