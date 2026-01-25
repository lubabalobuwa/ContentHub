using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.RestoreContent
{
    public class RestoreContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreContentHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(RestoreContentCommand command)
        {
            var validation = RestoreContentValidator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            if (content.Status != ContentStatus.Archived)
                return Result.Failure("Only archived content can be restored.");

            content.Restore(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
