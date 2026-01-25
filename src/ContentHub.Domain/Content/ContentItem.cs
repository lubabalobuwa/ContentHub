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
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }
        public DateTime? ArchivedAtUtc { get; private set; }

        private ContentItem() { }

        public ContentItem(string title, string body, Guid authorId, DateTime createdAtUtc)
        {
            Title = title;
            Body = body;
            AuthorId = authorId;
            Status = ContentStatus.Draft;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public void Update(string title, string body, DateTime updatedAtUtc)
        {
            if (Status == ContentStatus.Archived)
                throw new InvalidOperationException("Archived content cannot be updated");

            Title = title;
            Body = body;
            UpdatedAtUtc = updatedAtUtc;
        }

        public void Publish(DateTime publishedAtUtc)
        {
            if (Status != ContentStatus.Draft)
                throw new InvalidOperationException("Only draft content can be published");

            Status = ContentStatus.Published;
            UpdatedAtUtc = publishedAtUtc;
        }

        public void Archive(DateTime archivedAtUtc)
        {
            if (Status == ContentStatus.Archived)
                throw new InvalidOperationException("Content is already archived");

            Status = ContentStatus.Archived;
            ArchivedAtUtc = archivedAtUtc;
            UpdatedAtUtc = archivedAtUtc;
        }

        public void Restore(DateTime restoredAtUtc)
        {
            if (Status != ContentStatus.Archived)
                throw new InvalidOperationException("Only archived content can be restored");

            Status = ContentStatus.Draft;
            ArchivedAtUtc = null;
            UpdatedAtUtc = restoredAtUtc;
        }
    }
}
