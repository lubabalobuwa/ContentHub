using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Content;
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

        public async Task<ContentSummaryDto?> HandleAsync(GetContentByIdQuery query)
        {
            var content = await _repository.GetByIdAsync(query.ContentId);
            if (content is null)
                return null;

            return new ContentSummaryDto(
                content.Id,
                content.Title,
                content.Body,
                content.Status,
                content.RowVersion);
        }
    }
}
