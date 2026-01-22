using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetContentById
{
    public class GetContentByIdHandler
    {
        private readonly IContentReadRepository _repository;

        public GetContentByIdHandler(IContentReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<ContentItem> HandleAsync(GetContentByIdQuery query)
        {
            return await _repository.GetByIdAsync(query.ContentId);
        }
    }
}
