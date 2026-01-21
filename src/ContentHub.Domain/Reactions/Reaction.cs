using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Reactions
{
    public class Reaction : Entity
    {
        public Guid ContentItemId { get; private set; }
        public Guid UserId { get; private set; }

        private Reaction() { }

        public Reaction(Guid contentItemId, Guid userId)
        {
            ContentItemId = contentItemId;
            UserId = userId;
        }
    }
}
