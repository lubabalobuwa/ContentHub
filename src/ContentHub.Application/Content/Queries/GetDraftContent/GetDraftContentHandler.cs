using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetDraftContent
{
    public class GetDraftContentHandler
    {
        private readonly IContentReadRepository _repository;

        public GetDraftContentHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ContentSummaryDto>> HandleAsync()
        {
            var content = await _repository.GetDraftsAsync();

            return content.Select(x => new ContentSummaryDto(
                x.Id,
                x.Title,
                x.Body,
                x.Status,
                x.RowVersion)).ToList();
        }
    }
}
