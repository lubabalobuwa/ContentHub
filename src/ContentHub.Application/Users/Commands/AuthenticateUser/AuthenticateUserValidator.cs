using ContentHub.Application.Common;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public static class AuthenticateUserValidator
    {
        public static Result Validate(AuthenticateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.Password))
                return Result.Failure("Password is required.");

            return Result.Success();
        }
    }
}
