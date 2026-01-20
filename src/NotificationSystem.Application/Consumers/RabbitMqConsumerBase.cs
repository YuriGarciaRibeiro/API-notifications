using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Application.Options;
using NotificationSystem.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationSystem.Application.Consumers;

public abstract class RabbitMqConsumerBase<TMessage>(
    ILogger logger,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IServiceProvider serviceProvider,
    MessageProcessingMiddleware<TMessage> middleware) : BackgroundService
    where TMessage : class
{
    private readonly ILogger _logger = logger;
    private readonly RabbitMqOptions _rabbitMqOptions = rabbitMqOptions.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly MessageProcessingMiddleware<TMessage> _middleware = middleware;
    private IConnection? _connection;
    private IChannel? _channel;

    protected abstract string QueueName { get; }
    protected virtual string DeadLetterQueueName => $"{QueueName}-dlq";
    protected virtual string DeadLetterExchangeName => $"{QueueName}-dlx";

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

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var typedMessage = JsonSerializer.Deserialize<TMessage>(message, options);

                if (typedMessage == null)
                {
                    _logger.LogWarning("Failed to deserialize message. Moving to DLQ.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    return;
                }

                // Usar middleware para processar com retry e error handling
                var result = await _middleware.ProcessWithErrorHandlingAsync(
                    typedMessage,
                    ProcessMessageAsync,
                    GetNotificationIdsAsync,
                    GetChannelType(),
                    stoppingToken);

                if (result.IsSuccess)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                else
                {
                    // Falhou após todas as tentativas - enviar para DLQ
                    _logger.LogError(
                        "Message processing failed permanently. Moving to DLQ.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in message handler");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
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
}
