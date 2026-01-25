using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System;

namespace ContentHub.Application.Content.Commands.RestoreContent
{
    public class RestoreContentValidator : IValidator<RestoreContentCommand>
    {
        public Result Validate(RestoreContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

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
