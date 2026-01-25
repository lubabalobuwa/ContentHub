using ContentHub.Domain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IContentReadRepository
    {
        Task<ContentItem> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ContentItem>> GetArchivedAsync();
        Task<IReadOnlyList<ContentItem>> GetDraftsAsync();
        Task<IReadOnlyList<ContentItem>> GetPublishedAsync();
    }
}
