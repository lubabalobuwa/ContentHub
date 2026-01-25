using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<ContentSummaryDto>> HandleAsync()
        {
            var content = await _repository.GetPublishedAsync();

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
