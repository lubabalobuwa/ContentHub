using ContentHub.Application.Messaging;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Messaging
{
    public class NoOpRabbitMqPublisher : IRabbitMqPublisher
    {
        public Task PublishAsync(string queue, string message)
        {
            return Task.CompletedTask;
        }
    }
}
