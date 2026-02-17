using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Comments
{
    public class Comment : Entity
    {
        public Guid ContentItemId { get; private set; }
        public Guid UserId { get; private set; }
        public string Text { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        private Comment() { }

        public Comment(Guid contentItemId, Guid userId, string text, DateTime createdAtUtc)
        {
            ContentItemId = contentItemId;
            UserId = userId;
            Text = text;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
