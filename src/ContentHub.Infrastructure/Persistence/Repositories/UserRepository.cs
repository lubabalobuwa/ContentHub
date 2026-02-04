using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Users.Queries.GetUsers;
using ContentHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public UserRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(x => x.EmailVerificationTokenHash == tokenHash);
        }

        public async Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(x => x.PasswordResetTokenHash == tokenHash);
        }

        public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(
            int page,
            int pageSize,
            string? search)
        {
            var query = _dbContext.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.Email.ToLower().Contains(term) ||
                    x.DisplayName.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserSummaryDto(
                    x.Id,
                    x.Email,
                    x.DisplayName,
                    x.Role.ToString(),
                    x.EmailConfirmed,
                    x.IsDisabled,
                    x.CreatedAtUtc,
                    x.LastLoginAtUtc))
                .ToListAsync();

            return new PagedResult<UserSummaryDto>(items, page, pageSize, totalCount);
        }

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }

    }
}
