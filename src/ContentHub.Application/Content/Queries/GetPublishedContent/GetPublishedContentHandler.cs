using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IReadOnlyList<ContentItem>> HandleAsync()
        {
            return await _repository.GetPublishedAsync();
        }
    }
}
