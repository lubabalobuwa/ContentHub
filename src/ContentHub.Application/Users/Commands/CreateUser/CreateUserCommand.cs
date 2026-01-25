using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand(string Email, string DisplayName, string Password);
}
