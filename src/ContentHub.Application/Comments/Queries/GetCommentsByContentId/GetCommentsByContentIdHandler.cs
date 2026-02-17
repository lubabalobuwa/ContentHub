using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentHub.Application.Comments.Queries.GetCommentsByContentId
{
    public class GetCommentsByContentIdHandler
    {
        private readonly ICommentReadRepository _repository;

        public GetCommentsByContentIdHandler(ICommentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<CommentDto>> HandleAsync(GetCommentsByContentIdQuery query)
        {
            return await _repository.GetByContentIdAsync(query.ContentItemId);
        }
    }
}
