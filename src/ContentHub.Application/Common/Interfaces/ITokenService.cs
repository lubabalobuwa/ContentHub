using ContentHub.Domain.Users;

namespace ContentHub.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
        TimeSpan GetRefreshTokenLifetime();
    }
}
