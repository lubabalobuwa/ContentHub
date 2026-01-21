using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public CreateContentHandler(IContentRepository contentRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _contentRepository = contentRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(CreateContentCommand command)
        {
            var user = await _userRepository.GetByIdAsync(command.AuthorId);

            if (user is null)
                return Result.Failure("Author not found.");

            var content = new ContentItem(
                command.Title, command.Body, command.AuthorId);

            await _contentRepository.AddAsync(content);
            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
