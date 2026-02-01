using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.ResendVerification
{
    public class ResendVerificationHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly AuthSettings _authSettings;
        private readonly IUnitOfWork _unitOfWork;

        public ResendVerificationHandler(
            IUserRepository userRepository,
            ITokenService tokenService,
            IEmailSender emailSender,
            IOptions<AuthSettings> authOptions,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _authSettings = authOptions.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(ResendVerificationCommand command)
        {
            if (!_authSettings.RequireEmailVerification)
                return Result.Success();

            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null || user.EmailConfirmed)
                return Result.Success();

            var token = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService.HashRefreshToken(token);
            var expiresAt = DateTime.UtcNow.AddHours(_authSettings.EmailVerificationTokenHours);
            user.SetEmailVerificationToken(tokenHash, expiresAt);
            await _unitOfWork.CommitAsync();

            if (!string.IsNullOrWhiteSpace(_authSettings.BaseUrl))
            {
                var verifyLink = $"{_authSettings.BaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(token)}";
                var body = $@"
                    <p>Verify your email address:</p>
                    <p><a href=""{verifyLink}"">Verify email</a></p>";
                await _emailSender.SendAsync(email, "Verify your email", body);
            }

            return Result.Success();
        }
    }
}
