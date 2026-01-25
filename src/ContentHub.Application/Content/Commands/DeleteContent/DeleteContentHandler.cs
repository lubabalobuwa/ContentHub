using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.DeleteContent
{
    public class DeleteContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DeleteContentCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteContentHandler(
            IContentRepository contentRepository,
            ICurrentUserService currentUserService,
            IValidator<DeleteContentCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _currentUserService = currentUserService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(DeleteContentCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            var rowVersion = Convert.FromBase64String(command.RowVersion);
            _contentRepository.SetOriginalRowVersion(content, rowVersion);

            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("Unauthorized.");

            if (_currentUserService.Role != UserRole.Admin &&
                _currentUserService.UserId.Value != content.AuthorId)
                return Result.Failure("Forbidden.");

            if (content.Status == ContentStatus.Published)
                return Result.Failure("Published content must be archived before deletion.");

            _contentRepository.Remove(content);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
