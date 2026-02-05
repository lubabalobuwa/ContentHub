using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.ExternalLogin
{
    public class ExternalLoginValidator : IValidator<ExternalLoginCommand>
    {
        public Result Validate(ExternalLoginCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Provider))
                return Result.Failure("Provider is required.");

            if (string.IsNullOrWhiteSpace(command.ProviderUserId))
                return Result.Failure("Provider user id is required.");

            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.DisplayName))
                return Result.Failure("Display name is required.");

            return Result.Success();
        }
    }
}
