using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.ExternalLogin
{
    public record ExternalLoginCommand(
        string Provider,
        string ProviderUserId,
        string Email,
        string DisplayName,
        bool EmailVerified);
}
