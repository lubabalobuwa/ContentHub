using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Users;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.SetUserStatus
{
    public class SetUserStatusHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public SetUserStatusHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(SetUserStatusCommand command)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
                return Result.Failure("Unauthorized.");

            if (_currentUserService.Role != UserRole.Admin)
                return Result.Failure("Forbidden.");

            if (command.UserId == Guid.Empty)
                return Result.Failure("User not found.");

            if (command.UserId == _currentUserService.UserId.Value && command.IsDisabled)
                return Result.Failure("You cannot disable your own account.");

            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure("User not found.");

            if (command.IsDisabled)
                user.Disable();
            else
                user.Enable();

            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
    }
}
