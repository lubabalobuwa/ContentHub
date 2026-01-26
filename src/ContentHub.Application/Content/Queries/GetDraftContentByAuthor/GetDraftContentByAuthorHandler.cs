using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
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

        public async Task<PagedResult<ContentSummaryDto>> HandleAsync(GetDraftContentByAuthorQuery query)
        {
            var content = await _repository.GetDraftsByAuthorAsync(query.AuthorId, query.Page, query.PageSize);

            var items = content.Items.Select(x => new ContentSummaryDto(
                    x.Id,
                    x.AuthorId,
                    x.Title,
                    x.Body,
                    x.Status,
                    x.RowVersion))
                .ToList();

            return new PagedResult<ContentSummaryDto>(
                items,
                content.Page,
                content.PageSize,
                content.TotalCount);
        }
    }
}
