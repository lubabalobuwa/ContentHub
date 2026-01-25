using ContentHub.Application.Common;
using System;

namespace ContentHub.Application.Content.Commands.RestoreContent
{
    public static class RestoreContentValidator
    {
        public static Result Validate(RestoreContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            return Result.Success();
        }
    }
}
