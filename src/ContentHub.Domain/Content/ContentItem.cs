using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Content
{
    public class ContentItem : Entity
    {
        public string Title { get; private set; }
        public string Body { get; private set; }
        public Guid AuthorId { get; private set; }
        public ContentStatus Status { get; private set; }

        private ContentItem() { }

        public ContentItem(string title, string body, Guid authorId)
        {
            Title = title;
            Body = body;
            AuthorId = authorId;
            Status = ContentStatus.Draft;
        }

        public void Publish()
        {
            if (Status != ContentStatus.Draft)
                throw new InvalidOperationException("Only draft content can be published");

            Status = ContentStatus.Published;
        }
    }
}
