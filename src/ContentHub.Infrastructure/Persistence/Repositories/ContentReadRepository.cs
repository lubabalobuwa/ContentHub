using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<PagedResult<ContentItem>> GetArchivedAsync(int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Archived)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        public async Task<PagedResult<ContentItem>> GetArchivedByAuthorAsync(Guid authorId, int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Archived && x.AuthorId == authorId)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        public async Task<PagedResult<ContentItem>> GetDraftsAsync(int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Draft)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        public async Task<PagedResult<ContentItem>> GetDraftsByAuthorAsync(Guid authorId, int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Draft && x.AuthorId == authorId)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        public async Task<PagedResult<ContentItem>> GetPublishedByAuthorAsync(Guid authorId, int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Published && x.AuthorId == authorId)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        public async Task<PagedResult<ContentItem>> GetPublishedAsync(int page, int pageSize)
        {
            return await GetPagedAsync(
                _dbContext.ContentItems
                    .AsNoTracking()
                    .Where(x => x.Status == ContentStatus.Published)
                    .OrderByDescending(x => x.Id),
                page,
                pageSize);
        }

        private static async Task<PagedResult<ContentItem>> GetPagedAsync(
            IQueryable<ContentItem> query,
            int page,
            int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ContentItem>(items, page, pageSize, totalCount);
        }
    }
}
