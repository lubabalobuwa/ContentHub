using ContentHub.Application.Common.Interfaces;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<ContentSummaryDto>> HandleAsync()
        {
            var content = await _repository.GetArchivedAsync();

            return content.Select(x => new ContentSummaryDto(
                x.Id,
                x.Title,
                x.Body,
                x.Status,
                x.RowVersion)).ToList();
        }
    }
}
