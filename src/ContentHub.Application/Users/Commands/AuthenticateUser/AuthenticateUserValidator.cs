using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public class AuthenticateUserValidator : IValidator<AuthenticateUserCommand>
    {
        public Result Validate(AuthenticateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.Password))
                return Result.Failure("Password is required.");

            return Result.Success();
        }
    }
}
