using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.DeleteContent
{
    public class DeleteContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteContentHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(DeleteContentCommand command)
        {
            var validation = DeleteContentValidator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            if (content.Status == ContentStatus.Published)
                return Result.Failure("Published content must be archived before deletion.");

            _contentRepository.Remove(content);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
