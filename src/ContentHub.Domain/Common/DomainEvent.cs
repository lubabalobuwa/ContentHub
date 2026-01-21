using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Common
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
