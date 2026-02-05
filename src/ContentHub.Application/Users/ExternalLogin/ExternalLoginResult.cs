using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.ExternalLogin
{
    public record ExternalLoginResult(Guid UserId, string AccessToken, string RefreshToken, string Role);
}
