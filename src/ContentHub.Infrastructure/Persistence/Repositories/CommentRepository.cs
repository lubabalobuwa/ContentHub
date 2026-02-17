using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Comments;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public CommentRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Comment comment)
        {
            await _dbContext.Comments.AddAsync(comment);
        }
    }
}
