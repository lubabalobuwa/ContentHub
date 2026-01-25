using ContentHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ContentHub.Infrastructure.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(new object(), password);
        }

        public bool Verify(string hashedPassword, string password)
        {
            var result = _hasher.VerifyHashedPassword(new object(), hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
