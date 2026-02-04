using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public class AuthenticateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly AuthSettings _authSettings;
        private readonly IValidator<AuthenticateUserCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<AuthSettings> authOptions,
            IValidator<AuthenticateUserCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _authSettings = authOptions.Value;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthenticateUserResult>> HandleAsync(AuthenticateUserCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return Result<AuthenticateUserResult>.Failure(validation.Error);

            var email = command.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return Result<AuthenticateUserResult>.Failure("Invalid email or password.");

            if (!_passwordHasher.Verify(user.PasswordHash, command.Password))
                return Result<AuthenticateUserResult>.Failure("Invalid email or password.");

            if (user.IsDisabled)
                return Result<AuthenticateUserResult>.Failure("User disabled.");

            if (_authSettings.RequireEmailVerification && !user.EmailConfirmed)
                return Result<AuthenticateUserResult>.Failure("Email not verified.");

            user.MarkLoggedIn(DateTime.UtcNow);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = _tokenService.HashRefreshToken(refreshTokenValue);

            var now = DateTime.UtcNow;
            var refreshToken = new ContentHub.Domain.Users.RefreshToken(
                user.Id,
                refreshTokenHash,
                now,
                now.Add(_tokenService.GetRefreshTokenLifetime()));

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.CommitAsync();

            var accessToken = _tokenService.CreateAccessToken(user);

            return Result<AuthenticateUserResult>.Success(
                new AuthenticateUserResult(user.Id, accessToken, refreshTokenValue, user.Role.ToString()));
        }
    }
}
