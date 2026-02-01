using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;

namespace ContentHub.Application.Users.Commands.ResetPasswordWithToken
{
    public class ResetPasswordWithTokenValidator : IValidator<ResetPasswordWithTokenCommand>
    {
        public Result Validate(ResetPasswordWithTokenCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Token))
                return Result.Failure("Token is required.");

            if (string.IsNullOrWhiteSpace(command.NewPassword))
                return Result.Failure("NewPassword is required.");

            if (command.NewPassword.Length < 8)
                return Result.Failure("Password must be at least 8 characters.");

            return Result.Success();
        }
    }
}
