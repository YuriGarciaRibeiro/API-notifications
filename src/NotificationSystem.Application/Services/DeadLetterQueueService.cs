using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.DTOs.DeadLetter;
using NotificationSystem.Application.Interfaces;
using RabbitMQ.Client;

namespace NotificationSystem.Application.Services;

public class DeadLetterQueueService : IDeadLetterQueueService, IDisposable
{
    private readonly ILogger<DeadLetterQueueService> _logger;
    private readonly RabbitMqSettings _options;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private static readonly string[] _knownQueues =
    [
        "sms-notifications-dlq",
        "email-notifications-dlq",
        "push-notifications-dlq"
    ];

    public DeadLetterQueueService(
        ILogger<DeadLetterQueueService> logger,
        IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<DeadLetterQueueStatsDto>> GetAllDeadLetterQueueStatsAsync()
    {
        var stats = new List<DeadLetterQueueStatsDto>();

        foreach (var queueName in _knownQueues)
        {
            try
            {
                var declareResult = await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                stats.Add(new DeadLetterQueueStatsDto
                {
                    QueueName = queueName,
                    MessageCount = declareResult.MessageCount,
                    ConsumerCount = declareResult.ConsumerCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for queue {QueueName}", queueName);
            }
        }

        return stats;
    }

    public async Task<IEnumerable<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(
        string queueName,
        int limit = 100)
    {
        var messages = new List<DeadLetterMessageDto>();
        var messagesToRequeue = new List<(ulong deliveryTag, byte[] body, IReadOnlyBasicProperties properties)>();

        try
        {
            // Garantir que a fila existe e obter contagem
            var declareResult = await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Limitar ao número real de mensagens na fila
            var actualLimit = Math.Min(limit, (int)declareResult.MessageCount);

            // Ler mensagens sem remover permanentemente
            for (var i = 0; i < actualLimit; i++)
            {
                var result = await _channel.BasicGetAsync(queueName, false);

                if (result == null)
                    break; // Não há mais mensagens

                var messageBody = Encoding.UTF8.GetString(result.Body.ToArray());
                var headers = result.BasicProperties.Headers;

                messages.Add(new DeadLetterMessageDto
                {
                    QueueName = queueName,
                    MessageBody = messageBody,
                    DeliveryTag = result.DeliveryTag,
                    Timestamp = result.BasicProperties.Timestamp.UnixTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(result.BasicProperties.Timestamp.UnixTime).DateTime
                        : null,
                    Headers = headers?.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value ?? new object())!,
                    RetryCount = GetRetryCount(headers!)
                });

                // Guardar para requeue depois
                messagesToRequeue.Add((result.DeliveryTag, result.Body.ToArray(), result.BasicProperties));
            }

            // Recolocar todas as mensagens na fila na ordem correta
            foreach (var (deliveryTag, body, properties) in messagesToRequeue)
            {
                await _channel.BasicNackAsync(deliveryTag, false, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading messages from DLQ {QueueName}", queueName);
            throw;
        }

        return messages;
    }

    public async Task ReprocessMessageAsync(
        string dlqName,
        string originalQueueName,
        ulong deliveryTag)
    {
        try
        {
            // Pegar a mensagem da DLQ
            var result = await _channel.BasicGetAsync(dlqName, false);

            if (result == null)
            {
                _logger.LogWarning("Message with delivery tag {DeliveryTag} not found in {DLQ}",
                    deliveryTag, dlqName);
                return;
            }

            // Resetar o contador de retry
            var newProperties = new BasicProperties
            {
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    { "x-retry-count", 0 },
                    { "x-reprocessed-from-dlq", true },
                    { "x-reprocessed-at", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                }
            };

            // Republicar na fila original
            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: originalQueueName,
                mandatory: false,
                basicProperties: newProperties,
                body: result.Body);

            // Confirmar remoção da DLQ
            await _channel.BasicAckAsync(result.DeliveryTag, false);

            _logger.LogInformation(
                "Message reprocessed from {DLQ} to {OriginalQueue}",
                dlqName,
                originalQueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprocessing message from {DLQ}", dlqName);
            throw;
        }
    }

    public async Task ReprocessAllMessagesAsync(string dlqName, string originalQueueName)
    {
        try
        {
            var queueInfo = await _channel.QueueDeclareAsync(
                queue: dlqName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var messageCount = queueInfo.MessageCount;

            _logger.LogInformation(
                "Reprocessing {Count} messages from {DLQ} to {OriginalQueue}",
                messageCount,
                dlqName,
                originalQueueName);

            for (uint i = 0; i < messageCount; i++)
            {
                var result = await _channel.BasicGetAsync(dlqName, false);

                if (result == null)
                    break;

                var newProperties = new BasicProperties
                {
                    Persistent = true,
                    Headers = new Dictionary<string, object?>
                    {
                        { "x-retry-count", 0 },
                        { "x-reprocessed-from-dlq", true },
                        { "x-reprocessed-at", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                    }
                };

                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: originalQueueName,
                    mandatory: false,
                    basicProperties: newProperties,
                    body: result.Body);

                await _channel.BasicAckAsync(result.DeliveryTag, false);
            }

            _logger.LogInformation("Reprocessed all messages from {DLQ}", dlqName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprocessing all messages from {DLQ}", dlqName);
            throw;
        }
    }

    public async Task PurgeDeadLetterQueueAsync(string queueName)
    {
        try
        {
            await _channel.QueuePurgeAsync(queueName);
            _logger.LogInformation("Purged all messages from {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging queue {QueueName}", queueName);
            throw;
        }
    }

    private static int GetRetryCount(IDictionary<string, object>? headers)
    {
        return headers == null
            ? 0
            : headers.TryGetValue("x-retry-count", out var value)
                ? value switch
                {
                    int intValue => intValue,
                    byte[] byteValue => BitConverter.ToInt32(byteValue, 0),
                    _ => 0
                }
                : 0;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
