using ContentHub.Domain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IContentRepository
    {
        Task AddAsync(ContentItem contentItem);
        Task<ContentItem?> GetByIdAsync(Guid id);
        void Remove(ContentItem contentItem);
        void SetOriginalRowVersion(ContentItem contentItem, byte[] rowVersion);
    }
}
