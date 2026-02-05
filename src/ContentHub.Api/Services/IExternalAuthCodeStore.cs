using ContentHub.Api.Contracts.Responses;
using System;
using System.Threading.Tasks;

namespace ContentHub.Api.Services
{
    public interface IExternalAuthCodeStore
    {
        Task StoreAsync(string code, AuthResponse response, TimeSpan ttl);
        Task<AuthResponse?> TryConsumeAsync(string code);
    }
}
