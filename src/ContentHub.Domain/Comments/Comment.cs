using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Entities
{
    public class Comment : Entity
    {
        public Guid ContentItemId { get; private set; }
        public Guid UserId { get; private set; }
        public string Text { get; private set; }

        private Comment() { }

        public Comment(Guid contentItemId, Guid userId, string text)
        {
            ContentItemId = contentItemId;
            UserId = userId;
            Text = text;
        }
    }
}
