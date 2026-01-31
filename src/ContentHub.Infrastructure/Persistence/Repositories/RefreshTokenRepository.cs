using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Users;
using ContentHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public RefreshTokenRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _dbContext.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        }
    }
}
