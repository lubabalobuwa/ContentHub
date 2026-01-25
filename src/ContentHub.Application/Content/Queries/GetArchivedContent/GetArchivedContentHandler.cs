using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<ContentItem>> HandleAsync()
        {
            return await _repository.GetArchivedAsync();
        }
    }
}
