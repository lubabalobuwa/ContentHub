using ContentHub.Domain.Users;
using System;

namespace ContentHub.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        UserRole? Role { get; }
        bool IsAuthenticated { get; }
    }
}
