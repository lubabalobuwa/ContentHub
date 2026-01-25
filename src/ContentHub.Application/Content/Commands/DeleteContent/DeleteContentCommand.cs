using System;

namespace ContentHub.Application.Content.Commands.DeleteContent
{
    public record DeleteContentCommand(Guid ContentId);
}
