using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;

namespace ContentHub.Application.Comments.Commands.CreateComment
{
    public class CreateCommentValidator : IValidator<CreateCommentCommand>
    {
        public Result Validate(CreateCommentCommand command)
        {
            if (command.ContentItemId == Guid.Empty)
                return Result.Failure("ContentItemId is required.");

            if (string.IsNullOrWhiteSpace(command.Text))
                return Result.Failure("Comment text is required.");

            if (command.Text.Length > 2000)
                return Result.Failure("Comment must be under 2000 characters.");

            return Result.Success();
        }
    }
}
