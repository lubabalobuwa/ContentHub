namespace ContentHub.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string message, CancellationToken cancellationToken = default);
}
