using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System;

namespace ContentHub.Application.Content.Commands.UpdateContent
{
    public class UpdateContentValidator : IValidator<UpdateContentCommand>
    {
        public Result Validate(UpdateContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            if (string.IsNullOrWhiteSpace(command.Title))
                return Result.Failure("Title is required.");

            if (command.Title.Length > 200)
                return Result.Failure("Title must be under 200 characters.");

            if (string.IsNullOrWhiteSpace(command.Body))
                return Result.Failure("Body is required.");

            if (string.IsNullOrWhiteSpace(command.RowVersion))
                return Result.Failure("RowVersion is required.");

            if (!TryParseRowVersion(command.RowVersion))
                return Result.Failure("RowVersion is invalid.");

            return Result.Success();
        }

        private static bool TryParseRowVersion(string rowVersion)
        {
            try
            {
                _ = Convert.FromBase64String(rowVersion);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
