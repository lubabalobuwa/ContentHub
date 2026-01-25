using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.ArchiveContent
{
    public class ArchiveContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ArchiveContentHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
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

            if (content.Status == ContentStatus.Archived)
                return Result.Failure("Content is already archived.");

            content.Archive(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
