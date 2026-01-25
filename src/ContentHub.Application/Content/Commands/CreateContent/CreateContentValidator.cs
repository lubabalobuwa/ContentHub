using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;

namespace ContentHub.Application.Content.Commands.CreateContent
{
    public class CreateContentValidator : IValidator<CreateContentCommand>
    {
        public Result Validate(CreateContentCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Title))
                return Result.Failure("Title is required.");

            if (command.Title.Length > 200)
                return Result.Failure("Title must be under 200 characters.");

            if (string.IsNullOrWhiteSpace(command.Body))
                return Result.Failure("Body is required.");

            if (command.AuthorId == Guid.Empty)
                return Result.Failure("AuthorId is required.");

            return Result.Success();
        }
    }
}
