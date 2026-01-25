using ContentHub.Domain.Users;

namespace ContentHub.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
