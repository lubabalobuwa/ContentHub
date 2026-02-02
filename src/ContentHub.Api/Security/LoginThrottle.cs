using Microsoft.Extensions.Caching.Memory;
using System;

namespace ContentHub.Api.Security
{
    public class LoginThrottle
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(5);
        private const int MaxAttempts = 5;

        public LoginThrottle(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsLockedOut(string key)
        {
            if (_cache.TryGetValue<LockoutState>(key, out var state))
            {
                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc > DateTime.UtcNow)
                    return true;
            }

            return false;
        }

        public void RegisterFailure(string key)
        {
            var state = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _window;
                return new LockoutState();
            });

            state!.Failures++;
            if (state.Failures >= MaxAttempts)
                state.LockedUntilUtc = DateTime.UtcNow.Add(_window);

            _cache.Set(key, state, _window);
        }

        public void RegisterSuccess(string key)
        {
            _cache.Remove(key);
        }

        private sealed class LockoutState
        {
            public int Failures { get; set; }
            public DateTime? LockedUntilUtc { get; set; }
        }
    }
}
