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
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> HandleAsync(CreateUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Result<Guid>.Failure("Email is required.");

            if (string.IsNullOrWhiteSpace(command.DisplayName))
                return Result<Guid>.Failure("DisplayName is required.");

            var user = new User(command.Email.Trim(), command.DisplayName.Trim(), UserRole.Author);

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(user.Id);
        }
    }
}