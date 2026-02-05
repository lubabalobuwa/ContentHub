using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.ExternalLogin
{
    public class ExternalLoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IValidator<ExternalLoginCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public ExternalLoginHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, IValidator<ExternalLoginCommand> validator, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ExternalLoginResult>> HandleAsync(ExternalLoginCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return Result<ExternalLoginResult>.Failure(validation.Error);

            var provider = command.Provider.Trim();
            var providerUserId = command.ProviderUserId.Trim();
            var email = command.Email.Trim().ToLowerInvariant();
            var displayName = command.DisplayName.Trim();

            var user = await _userRepository.GetByExternalLoginAsync(provider, providerUserId)
                ?? await _userRepository.GetByEmailAsync(email);

            var isGoogle = provider.Equals("Google", StringComparison.OrdinalIgnoreCase);

            if (user is null)
            {
                var passwordHash = _passwordHasher.Hash(Guid.NewGuid().ToString("N"));
                user = new User(email, displayName, UserRole.Author, passwordHash, DateTime.UtcNow);
                if (isGoogle)
                {
                    user.ConfirmEmail();
                }

                await _userRepository.AddAsync(user);
            }
            else if (!user.EmailConfirmed && isGoogle)
            {
                user.ConfirmEmail();
            }

            if (user.IsDisabled)
                return Result<ExternalLoginResult>.Failure("User disabled.");

            user.AddExternalLogin(provider, providerUserId, email, DateTime.UtcNow);
            user.MarkLoggedIn(DateTime.UtcNow);

            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.HashRefreshToken(refreshTokenValue);
            var now = DateTime.UtcNow;

            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenHash,
                now,
                now.Add(_tokenService.GetRefreshTokenLifetime()));

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.CommitAsync();

            var accessToken = _tokenService.CreateAccessToken(user);

            return Result<ExternalLoginResult>.Success(
                new ExternalLoginResult(user.Id, accessToken, refreshTokenValue, user.Role.ToString()));
        }
    }
}
