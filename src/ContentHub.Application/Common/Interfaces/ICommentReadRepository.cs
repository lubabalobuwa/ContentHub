using ContentHub.Application.Comments.Queries.GetCommentsByContentId;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface ICommentReadRepository
    {
        Task<IReadOnlyList<CommentDto>> GetByContentIdAsync(Guid contentItemId);
    }
}
