using ContentHub.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ContentHub.Application.Content.Queries.GetContentById
{
    public class GetContentByIdHandler
    {
        private readonly IContentReadRepository _repository;
        private readonly IUserRepository _userRepository;

        public GetContentByIdHandler(
            IContentReadRepository repository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<ContentDetailDto?> HandleAsync(GetContentByIdQuery query)
        {
            var content = await _repository.GetByIdAsync(query.ContentId);
            if (content is null)
                return null;

            var author = await _userRepository.GetByIdAsync(content.AuthorId);
            if (author is null)
                return null;

            return new ContentDetailDto(
                content.Id,
                content.AuthorId,
                author.DisplayName,
                author.ProfileImageUrl,
                content.Title,
                content.Body,
                content.Status,
                content.ImageUrl,
                content.CreatedAtUtc,
                content.PublishedAtUtc,
                content.RowVersion);
        }
    }
}
