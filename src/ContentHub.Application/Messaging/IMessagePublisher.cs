namespace ContentHub.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string queue, string message, CancellationToken cancellationToken = default);
}
