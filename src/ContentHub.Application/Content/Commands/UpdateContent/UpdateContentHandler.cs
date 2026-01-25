using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.UpdateContent
{
    public class UpdateContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateContentHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(UpdateContentCommand command)
        {
            var validation = UpdateContentValidator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            if (content.Status == ContentStatus.Archived)
                return Result.Failure("Archived content cannot be updated.");

            content.Update(command.Title, command.Body);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
