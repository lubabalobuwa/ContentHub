using ContentHub.Application.Common;
using ContentHub.Application.Common.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetPublishedContent
{
    public class GetPublishedContentHandler
    {
        private readonly IContentReadRepository _repository;

        public GetPublishedContentHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<ContentSummaryDto>> HandleAsync(int page, int pageSize, string? search)
        {
            var content = await _repository.GetPublishedAsync(page, pageSize, search);

            var items = content.Items.Select(x => new ContentSummaryDto(
                    x.Id,
                    x.AuthorId,
                    x.Title,
                    x.Body,
                    x.Status,
                    x.ImageUrl,
                    x.CreatedAtUtc,
                    x.PublishedAtUtc,
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
