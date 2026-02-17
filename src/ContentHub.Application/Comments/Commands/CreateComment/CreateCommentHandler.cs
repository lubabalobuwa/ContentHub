using ContentHub.Application.Comments.Queries.GetCommentsByContentId;
using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Comments;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Comments.Commands.CreateComment
{
    public class CreateCommentHandler
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IContentReadRepository _contentReadRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateCommentCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCommentHandler(
            ICommentRepository commentRepository,
            IContentReadRepository contentReadRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IValidator<CreateCommentCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _commentRepository = commentRepository;
            _contentReadRepository = contentReadRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CommentDto>> HandleAsync(CreateCommentCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return Result<CommentDto>.Failure(validation.Error);

            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result<CommentDto>.Failure("Unauthorized.");

            var content = await _contentReadRepository.GetByIdAsync(command.ContentItemId);
            if (content is null)
                return Result<CommentDto>.Failure("Content not found.");

            if (content.Status != ContentStatus.Published)
                return Result<CommentDto>.Failure("Content is not published.");

            var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value);
            if (user is null)
                return Result<CommentDto>.Failure("User not found.");

            var now = DateTime.UtcNow;
            var comment = new Comment(content.Id, user.Id, command.Text.Trim(), now);

            await _commentRepository.AddAsync(comment);
            await _unitOfWork.CommitAsync();

            return Result<CommentDto>.Success(new CommentDto(
                comment.Id,
                comment.ContentItemId,
                comment.UserId,
                user.DisplayName,
                user.ProfileImageUrl,
                comment.Text,
                comment.CreatedAtUtc));
        }
    }
}
