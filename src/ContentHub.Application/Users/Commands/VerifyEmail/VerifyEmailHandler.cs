using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.VerifyEmail
{
    public class VerifyEmailHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly AuthSettings _authSettings;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailHandler(
            IUserRepository userRepository,
            ITokenService tokenService,
            IOptions<AuthSettings> authOptions,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _authSettings = authOptions.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(VerifyEmailCommand command)
        {
            if (!_authSettings.RequireEmailVerification)
                return Result.Success();

            if (string.IsNullOrWhiteSpace(command.Token))
                return Result.Failure("Token is required.");

            var tokenHash = _tokenService.HashRefreshToken(command.Token);
            var user = await _userRepository.GetByEmailVerificationTokenHashAsync(tokenHash);
            if (user is null)
                return Result.Failure("Invalid token.");

            if (user.EmailVerificationExpiresAtUtc is null || user.EmailVerificationExpiresAtUtc < DateTime.UtcNow)
                return Result.Failure("Token has expired.");

            user.ConfirmEmail();
            await _unitOfWork.CommitAsync();

            return Result.Success();
        }
    }
}
