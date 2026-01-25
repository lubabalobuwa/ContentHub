using ContentHub.Application.Common.Interfaces;
using ContentHub.Domain.Users;
using System;
using System.Security.Claims;

namespace ContentHub.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }

        public UserRole? Role
        {
            get
            {
                var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
                return Enum.TryParse<UserRole>(role, out var parsed) ? parsed : null;
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
