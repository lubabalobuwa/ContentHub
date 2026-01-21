using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Content
{
    public class ContentCreatedEvent : DomainEvent
    {
        public Guid ContentId { get; }

        public ContentCreatedEvent(Guid contentId)
        {
            ContentId = contentId;
        }
    }
}
