using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationSystem.Apllication.Interfaces;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.Interfaces;
using RabbitMQ.Client;

namespace NotificationSystem.Infrastructure.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public RabbitMQPublisher(IOptions<RabbitMqSettings> options)
    {
        var settings = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            UserName = settings.Username,
            Password = settings.Password,
            Port = settings.Port
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Declare queues
        DeclareQueuesAsync().GetAwaiter().GetResult();
    }

    private async Task DeclareQueuesAsync()
    {
        // Declare queues with the same DLX configuration as consumers
        await DeclareQueueWithDlxAsync("email-notifications");
        await DeclareQueueWithDlxAsync("sms-notifications");
        await DeclareQueueWithDlxAsync("push-notifications");
        await DeclareQueueWithDlxAsync("bulk-notification");
    }

    private async Task DeclareQueueWithDlxAsync(string queueName)
    {
        var deadLetterExchangeName = $"{queueName}-dlx";
        var deadLetterQueueName = $"{queueName}-dlq";

        // Declare Dead Letter Exchange
        await _channel.ExchangeDeclareAsync(
            exchange: deadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null
        );

        // Declare Dead Letter Queue
        await _channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind DLQ to DLX
        await _channel.QueueBindAsync(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: queueName,
            arguments: null
        );

        // Declare main queue with DLX configuration
        var queueArguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", deadLetterExchangeName },
            { "x-dead-letter-routing-key", queueName }
        };

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments
        );
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        _disposed = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        _disposed = true;
    }
}
