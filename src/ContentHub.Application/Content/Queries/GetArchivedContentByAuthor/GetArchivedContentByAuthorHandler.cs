using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetArchivedContentByAuthor
{
    public class GetArchivedContentByAuthorHandler
    {
        private readonly IContentReadRepository _repository;

        public GetArchivedContentByAuthorHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ContentItem>> HandleAsync(GetArchivedContentByAuthorQuery query)
        {
            return await _repository.GetArchivedByAuthorAsync(query.AuthorId);
        }
    }
}
