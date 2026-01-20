using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.Services;

public class DeadLetterQueueMonitorService(
    ILogger<DeadLetterQueueMonitorService> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<DeadLetterQueueMonitorService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    private readonly uint _alertThreshold = 10; // Alertar se DLQ tiver mais de 10 mensagens

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dead Letter Queue Monitor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorDeadLetterQueuesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring dead letter queues");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Dead Letter Queue Monitor stopped");
    }

    private async Task MonitorDeadLetterQueuesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dlqService = scope.ServiceProvider.GetRequiredService<IDeadLetterQueueService>();

        var stats = await dlqService.GetAllDeadLetterQueueStatsAsync();

        foreach (var stat in stats)
        {
            if (stat.MessageCount > 0)
            {
                if (stat.MessageCount >= _alertThreshold)
                {
                    _logger.LogWarning(
                        "⚠️ ALERT: Dead Letter Queue '{QueueName}' has {MessageCount} messages (threshold: {Threshold})",
                        stat.QueueName,
                        stat.MessageCount,
                        _alertThreshold);

                    // Aqui você pode adicionar integração com:
                    // - Envio de email de alerta
                    // - Notificação no Slack/Teams
                    // - PagerDuty
                    // - Outras ferramentas de alerting
                }
                else
                {
                    _logger.LogInformation(
                        "Dead Letter Queue '{QueueName}' has {MessageCount} messages",
                        stat.QueueName,
                        stat.MessageCount);
                }
            }
        }
    }
}
