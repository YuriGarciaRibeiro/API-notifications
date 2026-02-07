using Hangfire;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Messages;

namespace NotificationSystem.Application.Services;

public class CampaignSchedulerService : ICampaignSchedulerService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IMessagePublisher _messagePublisher;

    public CampaignSchedulerService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IMessagePublisher messagePublisher)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _messagePublisher = messagePublisher;
    }

    public string ScheduleCampaign(Guid jobId, DateTime scheduledFor)
    {
        // Agenda job para publicar mensagem na fila no horário especificado
        var hangfireJobId = _backgroundJobClient.Schedule(
            () => PublishBulkNotificationJob(jobId),
            scheduledFor);

        return hangfireJobId;
    }

    public void ScheduleRecurringCampaign(Guid jobId, string cronExpression, string? timeZone = null)
    {
        var recurringJobId = $"campaign-{jobId}";
        var timeZoneInfo = !string.IsNullOrEmpty(timeZone)
            ? TimeZoneInfo.FindSystemTimeZoneById(timeZone)
            : TimeZoneInfo.Utc;

        _recurringJobManager.AddOrUpdate(
            recurringJobId,
            () => PublishBulkNotificationJob(jobId),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = timeZoneInfo
            });
    }

    public void CancelScheduledCampaign(string hangfireJobId)
    {
        _backgroundJobClient.Delete(hangfireJobId);
    }

    public void RemoveRecurringCampaign(string recurringJobId)
    {
        _recurringJobManager.RemoveIfExists(recurringJobId);
    }

    /// <summary>
    /// Método executado pelo Hangfire que publica mensagem no RabbitMQ
    /// </summary>
    public async Task PublishBulkNotificationJob(Guid jobId)
    {
        var message = new BulkNotificationJobMessage(jobId);
        await _messagePublisher.PublishAsync("bulk-notifications", message, CancellationToken.None);
    }
}
