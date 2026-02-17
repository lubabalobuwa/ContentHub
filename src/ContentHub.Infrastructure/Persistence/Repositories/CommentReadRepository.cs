using ContentHub.Application.Comments.Queries.GetCommentsByContentId;
using ContentHub.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Persistence.Repositories
{
    public class CommentReadRepository : ICommentReadRepository
    {
        private readonly ContentHubDbContext _dbContext;

        public CommentReadRepository(ContentHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<CommentDto>> GetByContentIdAsync(Guid contentItemId)
        {
            var comments = await _dbContext.Comments
                .AsNoTracking()
                .Where(x => x.ContentItemId == contentItemId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            if (comments.Count == 0)
                return Array.Empty<CommentDto>();

            var userIds = comments
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName, x.ProfileImageUrl })
                .ToListAsync();

            var userLookup = users.ToDictionary(x => x.Id, x => x);

            return comments.Select(comment =>
            {
                userLookup.TryGetValue(comment.UserId, out var user);
                return new CommentDto(
                    comment.Id,
                    comment.ContentItemId,
                    comment.UserId,
                    user?.DisplayName ?? "Unknown",
                    user?.ProfileImageUrl,
                    comment.Text,
                    comment.CreatedAtUtc);
            }).ToList();
        }
    }
}
