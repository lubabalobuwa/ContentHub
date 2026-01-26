using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
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

        public async Task<PagedResult<ContentSummaryDto>> HandleAsync(GetArchivedContentByAuthorQuery query)
        {
            var content = await _repository.GetArchivedByAuthorAsync(query.AuthorId, query.Page, query.PageSize);

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
