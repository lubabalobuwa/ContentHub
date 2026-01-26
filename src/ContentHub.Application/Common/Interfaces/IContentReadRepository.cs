using ContentHub.Application.Common;
using ContentHub.Domain.Content;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IContentReadRepository
    {
        Task<ContentItem> GetByIdAsync(Guid id);
        Task<PagedResult<ContentItem>> GetArchivedAsync(int page, int pageSize);
        Task<PagedResult<ContentItem>> GetArchivedByAuthorAsync(Guid authorId, int page, int pageSize);
        Task<PagedResult<ContentItem>> GetDraftsAsync(int page, int pageSize);
        Task<PagedResult<ContentItem>> GetDraftsByAuthorAsync(Guid authorId, int page, int pageSize);
        Task<PagedResult<ContentItem>> GetPublishedByAuthorAsync(Guid authorId, int page, int pageSize);
        Task<PagedResult<ContentItem>> GetPublishedAsync(int page, int pageSize);
    }
}
