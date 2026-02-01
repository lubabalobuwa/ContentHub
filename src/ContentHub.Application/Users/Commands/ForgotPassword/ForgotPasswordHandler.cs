using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.ForgotPassword
{
    public class ForgotPasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly AuthSettings _authSettings;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordHandler(
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

        public async Task<Result> HandleAsync(ForgotPasswordCommand command)
        {
            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return Result.Success();

            var token = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService.HashRefreshToken(token);
            var expiresAt = DateTime.UtcNow.AddHours(_authSettings.PasswordResetTokenHours);
            user.SetPasswordResetToken(tokenHash, expiresAt);
            await _unitOfWork.CommitAsync();

            if (!string.IsNullOrWhiteSpace(_authSettings.BaseUrl))
            {
                var resetLink = $"{_authSettings.BaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
                var body = $@"
                    <p>You requested a password reset.</p>
                    <p><a href=""{resetLink}"">Reset password</a></p>";
                await _emailSender.SendAsync(email, "Reset your password", body);
            }

            return Result.Success();
        }
    }
}
