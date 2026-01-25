using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System;

namespace ContentHub.Application.Users.Commands.CreateUser
{
    public class CreateUserValidator : IValidator<CreateUserCommand>
    {
        public Result Validate(CreateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.DisplayName))
                return Result.Failure("DisplayName is required.");

            if (string.IsNullOrWhiteSpace(command.Password))
                return Result.Failure("Password is required.");

            if (command.Password.Length < 8)
                return Result.Failure("Password must be at least 8 characters.");

            return Result.Success();
        }
    }
}
