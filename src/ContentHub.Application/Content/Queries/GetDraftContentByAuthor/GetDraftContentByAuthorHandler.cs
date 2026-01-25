using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetDraftContentByAuthor
{
    public class GetDraftContentByAuthorHandler
    {
        private readonly IContentReadRepository _repository;

        public GetDraftContentByAuthorHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ContentItem>> HandleAsync(GetDraftContentByAuthorQuery query)
        {
            return await _repository.GetDraftsByAuthorAsync(query.AuthorId);
        }
    }
}
