using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationSystem.Application.Consumers;

public abstract class RabbitMqConsumerBase<TMessage> : BackgroundService
    where TMessage : class
{
    private readonly ILogger _logger;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    protected abstract string QueueName { get; }
    protected virtual string DeadLetterQueueName => $"{QueueName}-dlq";
    protected virtual string DeadLetterExchangeName => $"{QueueName}-dlx";

    protected RabbitMqConsumerBase(
        ILogger logger,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ConsumerName} starting...", GetType().Name);

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.Host,
            Port = _rabbitMqOptions.Port,
            UserName = _rabbitMqOptions.Username,
            Password = _rabbitMqOptions.Password,
            VirtualHost = _rabbitMqOptions.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Declare Dead Letter Exchange
        await _channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Declare Dead Letter Queue
        await _channel.QueueDeclareAsync(
            queue: DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Bind DLQ to DLX
        await _channel.QueueBindAsync(
            queue: DeadLetterQueueName,
            exchange: DeadLetterExchangeName,
            routingKey: QueueName,
            arguments: null,
            cancellationToken: cancellationToken);

        // Declare main queue with DLX configuration
        var queueArguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", DeadLetterExchangeName },
            { "x-dead-letter-routing-key", QueueName }
        };

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Connected to RabbitMQ. Queue '{QueueName}' declared with DLQ support",
            QueueName);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("Channel is not initialized");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            TMessage? typedMessage = null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                typedMessage = JsonSerializer.Deserialize<TMessage>(message, options);

                if (typedMessage == null)
                {
                    _logger.LogWarning("Failed to deserialize message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    return;
                }

                var retryCount = GetRetryCount(ea.BasicProperties);

                _logger.LogInformation(
                    "Processing message in {QueueName} (Attempt {Attempt}/{MaxAttempts})",
                    QueueName,
                    retryCount + 1,
                    _rabbitMqOptions.MaxRetryAttempts);

                // Process the message (implemented by derived class)
                await ProcessMessageAsync(typedMessage, stoppingToken);

                _logger.LogInformation("Message processed successfully in {QueueName}", QueueName);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(ea, typedMessage, ex, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("{ConsumerName} is listening for messages on queue '{QueueName}'...",
            GetType().Name,
            QueueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ConsumerName} stopping...", GetType().Name);

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

    protected abstract Task<(Guid NotificationId, Guid ChannelId)> GetNotificationIdsAsync(TMessage message);

    protected abstract Type GetChannelType();

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers == null)
            return 0;

        if (properties.Headers.TryGetValue("x-retry-count", out var value))
        {
            return value switch
            {
                int intValue => intValue,
                byte[] byteValue => BitConverter.ToInt32(byteValue, 0),
                _ => 0
            };
        }

        return 0;
    }

    private async Task HandleFailureAsync(
        BasicDeliverEventArgs ea,
        TMessage? message,
        Exception ex,
        CancellationToken stoppingToken)
    {
        if (_channel == null)
            return;

        var retryCount = GetRetryCount(ea.BasicProperties);

        if (retryCount < _rabbitMqOptions.MaxRetryAttempts - 1)
        {
            // Still have retries left - requeue with incremented retry count
            _logger.LogWarning(
                ex,
                "Error processing message in {QueueName}. Retry {Retry}/{MaxRetries}. Requeueing message.",
                QueueName,
                retryCount + 1,
                _rabbitMqOptions.MaxRetryAttempts);

            // Reject without requeue (will not go to DLQ yet)
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);

            // Republish with updated retry count
            var newProperties = new BasicProperties
            {
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    { "x-retry-count", retryCount + 1 }
                }
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: QueueName,
                mandatory: false,
                basicProperties: newProperties,
                body: ea.Body,
                cancellationToken: stoppingToken);

            // Acknowledge the original message
            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        }
        else
        {
            // Max retries exceeded - mark as failed and send to DLQ
            _logger.LogError(
                ex,
                "Message in {QueueName} failed after {MaxRetries} attempts. Moving to DLQ.",
                QueueName,
                _rabbitMqOptions.MaxRetryAttempts);

            if (message != null)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    var (notificationId, channelId) = await GetNotificationIdsAsync(message);
                    var channelType = GetChannelType();

                    var method = typeof(INotificationRepository)
                        .GetMethod(nameof(INotificationRepository.UpdateNotificationChannelStatusAsync))
                        ?.MakeGenericMethod(channelType);

                    if (method != null)
                    {
                        await (Task)method.Invoke(
                            repository,
                            new object[] { notificationId, channelId, NotificationStatus.Failed })!;

                        _logger.LogInformation(
                            "Notification {NotificationId} marked as Failed in database",
                            notificationId);
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(
                        dbEx,
                        "Failed to update notification status to Failed");
                }
            }

            // Reject without requeue - will be sent to DLQ via DLX
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
        }
    }
}
