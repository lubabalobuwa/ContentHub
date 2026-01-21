using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string queue, string message);
    }
}
