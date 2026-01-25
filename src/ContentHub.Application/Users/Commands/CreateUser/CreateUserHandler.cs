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
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> HandleAsync(CreateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result<Guid>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.DisplayName))
                return Result<Guid>.Failure("DisplayName is required.");

            if (string.IsNullOrWhiteSpace(command.Password))
                return Result<Guid>.Failure("Password is required.");

            if (command.Password.Length < 8)
                return Result<Guid>.Failure("Password must be at least 8 characters.");

            var email = command.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser is not null)
                return Result<Guid>.Failure("Email is already registered.");

            var passwordHash = _passwordHasher.Hash(command.Password);
            var user = new User(email, command.DisplayName.Trim(), UserRole.Author, passwordHash);

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(user.Id);
        }
    }
}
