using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.ArchiveContent
{
    public class ArchiveContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public ArchiveContentHandler(
            IContentRepository contentRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(ArchiveContentCommand command)
        {
            var validation = ArchiveContentValidator.Validate(command);
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

            if (content.Status == ContentStatus.Archived)
                return Result.Failure("Content is already archived.");

            content.Archive(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
