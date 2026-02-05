using ContentHub.Api.Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ContentHub.Api.Services
{
    public class MemoryExternalAuthCodeStore : IExternalAuthCodeStore
    {
        private readonly IMemoryCache _cache;

        public MemoryExternalAuthCodeStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task StoreAsync(string code, AuthResponse response, TimeSpan ttl)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            _cache.Set(GetKey(code), response, options);
            return Task.CompletedTask;
        }

        public Task<AuthResponse?> TryConsumeAsync(string code)
        {
            var key = GetKey(code);
            if (!_cache.TryGetValue(key, out AuthResponse? response))
                return Task.FromResult<AuthResponse?>(null);

            _cache.Remove(key);
            return Task.FromResult(response);
        }

        private static string GetKey(string code) => $"external-auth:{code}";
    }
}
