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
    public class ContentRepository : IContentRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public ContentRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ContentItem contentItem)
        {
            await _dbContext.ContentItems.AddAsync(contentItem);
        }

        public async Task<ContentItem?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ContentItems
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public void Remove(ContentItem contentItem)
        {
            _dbContext.ContentItems.Remove(contentItem);
        }
    }
}
