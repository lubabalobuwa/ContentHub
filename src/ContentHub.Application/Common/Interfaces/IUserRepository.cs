using ContentHub.Application.Common;
using ContentHub.Application.Users.Queries.GetUsers;
using ContentHub.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash);
        Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash);
        Task<PagedResult<UserSummaryDto>> GetUsersAsync(
            int page,
            int pageSize,
            string? search);
        Task AddAsync(User user);
    }
}
