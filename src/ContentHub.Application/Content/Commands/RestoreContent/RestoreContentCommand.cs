using System;

namespace ContentHub.Application.Content.Commands.RestoreContent
{
    public record RestoreContentCommand(Guid ContentId);
}
