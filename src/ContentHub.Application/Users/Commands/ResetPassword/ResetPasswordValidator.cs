using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;

namespace ContentHub.Application.Users.Commands.ResetPassword
{
    public class ResetPasswordValidator : IValidator<ResetPasswordCommand>
    {
        public Result Validate(ResetPasswordCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.NewPassword))
                return Result.Failure("NewPassword is required.");

            if (command.NewPassword.Length < 8)
                return Result.Failure("Password must be at least 8 characters.");

            return Result.Success();
        }
    }
}
