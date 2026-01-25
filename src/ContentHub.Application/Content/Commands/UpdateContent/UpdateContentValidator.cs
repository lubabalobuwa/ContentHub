using ContentHub.Application.Common;
using System;

namespace ContentHub.Application.Content.Commands.UpdateContent
{
    public static class UpdateContentValidator
    {
        public static Result Validate(UpdateContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            if (string.IsNullOrWhiteSpace(command.Title))
                return Result.Failure("Title is required.");

            if (command.Title.Length > 200)
                return Result.Failure("Title must be under 200 characters.");

            if (string.IsNullOrWhiteSpace(command.Body))
                return Result.Failure("Body is required.");

            return Result.Success();
        }
    }
}
