using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Users.Commands.CreateUser;
using ContentHub.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<CreateUserCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IValidator<CreateUserCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
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

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(user.Id);
        }
    }
}
