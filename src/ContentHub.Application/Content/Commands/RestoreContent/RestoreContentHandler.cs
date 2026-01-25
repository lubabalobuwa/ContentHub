using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.RestoreContent
{
    public class RestoreContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<RestoreContentCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreContentHandler(
            IContentRepository contentRepository,
            ICurrentUserService currentUserService,
            IValidator<RestoreContentCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _currentUserService = currentUserService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(RestoreContentCommand command)
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

            if (content.Status != ContentStatus.Archived)
                return Result.Failure("Only archived content can be restored.");

            content.Restore(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
