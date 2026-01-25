using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Messaging;
using ContentHub.Domain.Users;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.PublishContent
{
    public class PublishContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRabbitMqPublisher _publisher;

        public PublishContentHandler(
        IContentRepository contentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IRabbitMqPublisher publisher)
        {
            _contentRepository = contentRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<Result> HandleAsync(PublishContentCommand command)
        {
            var content = await _contentRepository.GetByIdAsync(command.ContentId);
            if (content is null)
                return Result.Failure("Content not found.");

            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("Unauthorized.");

            if (_currentUserService.Role != UserRole.Admin &&
                _currentUserService.UserId.Value != content.AuthorId)
                return Result.Failure("Forbidden.");

            content.Publish(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();

            await _publisher.PublishAsync(
                queue: "content.published",
                message: command.ContentId.ToString());

            return Result.Success();
        }
    }
}
