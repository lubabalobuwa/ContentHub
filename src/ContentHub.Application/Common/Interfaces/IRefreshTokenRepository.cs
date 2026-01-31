using ContentHub.Domain.Users;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    }
}
