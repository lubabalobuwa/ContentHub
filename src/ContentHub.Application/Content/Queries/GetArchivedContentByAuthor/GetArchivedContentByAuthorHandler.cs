using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IReadOnlyList<ContentSummaryDto>> HandleAsync(GetArchivedContentByAuthorQuery query)
        {
            var content = await _repository.GetArchivedByAuthorAsync(query.AuthorId);

            return content.Select(x => new ContentSummaryDto(
                x.Id,
                x.Title,
                x.Body,
                x.Status,
                x.RowVersion)).ToList();
        }
    }
}
