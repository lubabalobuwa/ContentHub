using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.ResetPassword
{
    public class ResetPasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(ResetPasswordCommand command)
        {
            var validation = ResetPasswordValidator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return Result.Failure("User not found.");

            var hash = _passwordHasher.Hash(command.NewPassword);
            user.SetPasswordHash(hash);

            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
