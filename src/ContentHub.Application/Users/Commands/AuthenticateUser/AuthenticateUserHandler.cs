using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public class AuthenticateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IValidator<AuthenticateUserCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IValidator<AuthenticateUserCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
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

            user.MarkLoggedIn(DateTime.UtcNow);
            await _unitOfWork.CommitAsync();

            var token = _tokenService.CreateToken(user);

            return Result<AuthenticateUserResult>.Success(
                new AuthenticateUserResult(user.Id, token, user.Role.ToString()));
        }
    }
}
