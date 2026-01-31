using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RefreshTokenResult>> HandleAsync(RefreshTokenCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
                return Result<RefreshTokenResult>.Failure("Refresh token is required.");

            var tokenHash = _tokenService.HashRefreshToken(command.RefreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
            if (existing is null || !existing.IsActive)
                return Result<RefreshTokenResult>.Failure("Invalid refresh token.");

            var user = await _userRepository.GetByIdAsync(existing.UserId);
            if (user is null)
                return Result<RefreshTokenResult>.Failure("User not found.");

            var now = DateTime.UtcNow;
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshTokenValue);
            var newRefreshToken = new ContentHub.Domain.Users.RefreshToken(
                user.Id,
                newRefreshTokenHash,
                now,
                now.Add(_tokenService.GetRefreshTokenLifetime()));

            existing.Revoke(newRefreshTokenHash, now);
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.CommitAsync();

            var accessToken = _tokenService.CreateAccessToken(user);

            return Result<RefreshTokenResult>.Success(
                new RefreshTokenResult(user.Id, accessToken, newRefreshTokenValue, user.Role.ToString()));
        }
    }
}
