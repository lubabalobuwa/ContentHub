using ContentHub.Application.Messaging;
using RabbitMQ.Client;
using System.Text;

namespace ContentHub.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        public async Task PublishAsync(string queue, string message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queue,
                body: body);
        }
    }
}
