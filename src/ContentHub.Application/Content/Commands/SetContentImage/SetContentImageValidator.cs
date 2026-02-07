using ContentHub.Application.Common;

namespace ContentHub.Application.Content.Commands.SetContentImage
{
    public class SetContentImageValidator : IValidator<SetContentImageCommand>
    {
        public Result Validate(SetContentImageCommand command)
        {
            if (command.ContentId == Guid.Empty)
                return Result.Failure("ContentId is required.");

            if (string.IsNullOrWhiteSpace(command.ImageUrl))
                return Result.Failure("ImageUrl is required.");

            if (string.IsNullOrWhiteSpace(command.RowVersion))
                return Result.Failure("RowVersion is required.");

            return Result.Success();
        }
    }
}
