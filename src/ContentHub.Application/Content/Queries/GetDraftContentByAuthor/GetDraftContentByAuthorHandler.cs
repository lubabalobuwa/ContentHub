using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IReadOnlyList<ContentSummaryDto>> HandleAsync(GetDraftContentByAuthorQuery query)
        {
            var content = await _repository.GetDraftsByAuthorAsync(query.AuthorId);

            return content.Select(x => new ContentSummaryDto(
                x.Id,
                x.AuthorId,
                x.Title,
                x.Body,
                x.Status,
                x.RowVersion)).ToList();
        }
    }
}
