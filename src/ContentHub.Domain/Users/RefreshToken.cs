using ContentHub.Domain.Common;
using System;

namespace ContentHub.Domain.Users
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public string? ReplacedByTokenHash { get; private set; }

        private RefreshToken() { }

        public RefreshToken(Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
        {
            UserId = userId;
            TokenHash = tokenHash;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

        public void Revoke(string? replacedByTokenHash, DateTime revokedAtUtc)
        {
            RevokedAtUtc = revokedAtUtc;
            ReplacedByTokenHash = replacedByTokenHash;
        }
    }
}
