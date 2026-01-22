using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence.Repositories
{
    public class ContentReadRepository : IContentReadRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public ContentReadRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ContentItem> GetByIdAsync(Guid id)
        {
            return await _dbContext.ContentItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<ContentItem>> GetPublishedAsync()
        {
            return await _dbContext.ContentItems
                .AsNoTracking()
                .Where(x => x.Status == ContentStatus.Published)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }
    }
}
