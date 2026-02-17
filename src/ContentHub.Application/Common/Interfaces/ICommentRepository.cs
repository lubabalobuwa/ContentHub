using ContentHub.Domain.Comments;
using System;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
    }
}
