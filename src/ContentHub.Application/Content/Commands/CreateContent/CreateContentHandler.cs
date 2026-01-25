using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Users;
using ContentHub.Domain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Commands.CreateContent
{
    public class CreateContentHandler
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateContentCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateContentHandler(
            IContentRepository contentRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IValidator<CreateContentCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(CreateContentCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("Unauthorized.");

            if (_currentUserService.Role != UserRole.Admin &&
                _currentUserService.UserId.Value != command.AuthorId)
                return Result.Failure("Forbidden.");

            var user = await _userRepository.GetByIdAsync(command.AuthorId);

            if (user is null)
                return Result.Failure("Author not found.");

            var content = new ContentItem(
                command.Title, command.Body, command.AuthorId, DateTime.UtcNow);

            await _contentRepository.AddAsync(content);
            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
