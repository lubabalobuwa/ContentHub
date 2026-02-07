using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.SetUserProfileImage
{
    public class SetUserProfileImageHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetUserProfileImageHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(SetUserProfileImageCommand command)
        {
            if (command.UserId == Guid.Empty)
                return Result.Failure("UserId is required.");

            if (string.IsNullOrWhiteSpace(command.ImageUrl))
                return Result.Failure("ImageUrl is required.");

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure("User not found.");

            user.SetProfileImageUrl(command.ImageUrl);
            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
