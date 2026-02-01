using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using ContentHub.Application.Users.Commands.CreateUser;
using ContentHub.Domain.Users;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly AuthSettings _authSettings;
        private readonly IValidator<CreateUserCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailSender emailSender,
            IOptions<AuthSettings> authOptions,
            IValidator<CreateUserCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _authSettings = authOptions.Value;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> HandleAsync(CreateUserCommand command)
        {
            var validation = _validator.Validate(command);
            if (!validation.IsSuccess)
                return Result<Guid>.Failure(validation.Error);

            var email = command.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser is not null)
                return Result<Guid>.Failure("Email is already registered.");

            var passwordHash = _passwordHasher.Hash(command.Password);
            var user = new User(email, command.DisplayName.Trim(), UserRole.Author, passwordHash, DateTime.UtcNow);

            string? verificationToken = null;
            if (_authSettings.RequireEmailVerification)
            {
                verificationToken = _tokenService.GenerateRefreshToken();
                var tokenHash = _tokenService.HashRefreshToken(verificationToken);
                var expiresAt = DateTime.UtcNow.AddHours(_authSettings.EmailVerificationTokenHours);
                user.SetEmailVerificationToken(tokenHash, expiresAt);
            }
            else
            {
                user.ConfirmEmail();
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            if (verificationToken is not null && !string.IsNullOrWhiteSpace(_authSettings.BaseUrl))
            {
                var verifyLink = $"{_authSettings.BaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
                var body = $@"
                    <p>Welcome to TechContentHub!</p>
                    <p>Please verify your email by clicking the link below:</p>
                    <p><a href=""{verifyLink}"">Verify email</a></p>";

                await _emailSender.SendAsync(email, "Verify your email", body);
            }

            return Result<Guid>.Success(user.Id);
        }
    }
}
