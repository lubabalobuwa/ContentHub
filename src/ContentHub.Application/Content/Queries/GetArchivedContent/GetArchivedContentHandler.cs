using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetArchivedContent
{
    public class GetArchivedContentHandler
    {
        private readonly IContentReadRepository _repository;

        public GetArchivedContentHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<ContentSummaryDto>> HandleAsync(int page, int pageSize)
        {
            var content = await _repository.GetArchivedAsync(page, pageSize);

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
