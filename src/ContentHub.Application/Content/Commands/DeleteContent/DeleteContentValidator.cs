using ContentHub.Application.Common;
using System;

namespace ContentHub.Application.Content.Commands.DeleteContent
{
    public static class DeleteContentValidator
    {
        public static Result Validate(DeleteContentCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            return Result.Success();
        }
    }
}
