using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Content
{
    public class ContentPublishedEvent : DomainEvent
    {
        public Guid ContentId { get; }

        public ContentPublishedEvent(Guid contentId)
        {
            ContentId = contentId;
        }
    }
}
