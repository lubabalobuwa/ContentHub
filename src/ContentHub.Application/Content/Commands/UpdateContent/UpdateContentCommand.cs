using System;

namespace ContentHub.Application.Content.Commands.UpdateContent
{
    public record UpdateContentCommand(Guid ContentId, string Title, string Body);
}
