using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.ResetPasswordWithToken
{
    public class ResetPasswordWithTokenHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IValidator<ResetPasswordWithTokenCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordWithTokenHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IValidator<ResetPasswordWithTokenCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(ResetPasswordWithTokenCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return validation;

            var tokenHash = _tokenService.HashRefreshToken(command.Token);
            var user = await _userRepository.GetByPasswordResetTokenHashAsync(tokenHash);
            if (user is null)
                return Result.Failure("Invalid token.");

            if (user.PasswordResetExpiresAtUtc is null || user.PasswordResetExpiresAtUtc < DateTime.UtcNow)
                return Result.Failure("Token has expired.");

            var hash = _passwordHasher.Hash(command.NewPassword);
            user.SetPasswordHash(hash);
            user.MarkPasswordResetUsed(DateTime.UtcNow);

            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
    }
}
