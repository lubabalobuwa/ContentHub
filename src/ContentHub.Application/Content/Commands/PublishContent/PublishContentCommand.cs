using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.PublishContent
{
    public record PublishContentCommand(Guid ContentId, string RowVersion); 
}
