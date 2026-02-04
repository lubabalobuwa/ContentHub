using System;

namespace ContentHub.Application.Users.Commands.SetUserStatus
{
    public record SetUserStatusCommand(Guid UserId, bool IsDisabled);
}
