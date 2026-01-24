using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Worker.Consumers
{
    public class ContentPublishedConsumer : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync(stoppingToken);
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
            queue: "content.published",
            durable: false,
            exclusive: false,
            autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                Console.WriteLine($"Content published event received: {message}");

                await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            };

            await channel.BasicConsumeAsync("content.published", autoAck: false, consumer);

            await Task.CompletedTask;
        }
    }
}
