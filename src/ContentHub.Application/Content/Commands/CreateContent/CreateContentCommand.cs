using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.CreateContent
{
    public record CreateContentCommand(
        string Title,
        string Body,
        Guid AuthorId
    );
}
