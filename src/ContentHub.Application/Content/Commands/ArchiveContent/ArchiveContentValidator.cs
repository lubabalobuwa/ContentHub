using ContentHub.Application.Common;
using System;

namespace ContentHub.Application.Content.Commands.ArchiveContent
{
    public static class ArchiveContentValidator
    {
        public static Result Validate(ArchiveContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            return Result.Success();
        }
    }
}
