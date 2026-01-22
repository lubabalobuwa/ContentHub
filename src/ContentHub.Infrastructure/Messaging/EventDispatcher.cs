using ContentHub.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Messaging
{
    public class EventDispatcher
    {
        private readonly IRabbitMqPublisher _publisher;

        public EventDispatcher(IRabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task PublishContentPublishedAsync(Guid contentId)
        {
            return _publisher.PublishAsync(
                "content.published",
                contentId.ToString());
        }
    }
}
