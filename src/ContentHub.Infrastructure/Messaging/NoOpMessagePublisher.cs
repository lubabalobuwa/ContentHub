using ContentHub.Application.Messaging;

namespace ContentHub.Infrastructure.Messaging;

public sealed class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync(string message, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
