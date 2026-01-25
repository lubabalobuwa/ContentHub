using ContentHub.Application.Messaging;
using RabbitMQ.Client;
using System.Text;

namespace ContentHub.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IConnection _connection;

        public RabbitMqPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync(string queue, string message)
        {
            await using var channel = await _connection.CreateChannelAsync();

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
