namespace ContentHub.Infrastructure.Messaging;

public sealed class ServiceBusSettings
{
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string QueueName { get; set; } = "content-published";
}
