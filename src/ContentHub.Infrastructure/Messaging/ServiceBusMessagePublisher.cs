using Azure.Messaging.ServiceBus;
using ContentHub.Application.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentHub.Infrastructure.Messaging;

public sealed class ServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly string _queueName;
    private readonly ILogger<ServiceBusMessagePublisher> _logger;

    public ServiceBusMessagePublisher(
        ServiceBusClient client,
        IOptions<ServiceBusSettings> options,
        ILogger<ServiceBusMessagePublisher> logger)
    {
        _client = client;
        _queueName = options.Value.QueueName;
        _logger = logger;
    }

    public async Task PublishAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var sender = _client.CreateSender(_queueName);
            await sender.SendMessageAsync(new ServiceBusMessage(message)
            {
                ContentType = "text/plain",
                MessageId = message
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            // Publishing content must not report failure after the database commit succeeds.
            _logger.LogError(exception, "Failed to enqueue message. Queue={Queue} MessageId={MessageId}", _queueName, message);
        }
    }
}
