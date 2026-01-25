using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.PublishContent
{
    public class PublishContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRabbitMqPublisher _publisher;

        public PublishContentHandler(
        IContentRepository contentRepository,
        IUnitOfWork unitOfWork,
        IRabbitMqPublisher publisher)
        {
            _contentRepository = contentRepository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> HandleAsync(PublishContentCommand command)
        {
            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            content.Publish(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            await _publisher.PublishAsync(
                queue: "content.published",
                message: command.ContentId.ToString());

            return Result.Success();
        }
    }
}
