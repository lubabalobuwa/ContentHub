using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Users
{
    public class User : Entity
    {
        public string Email { get; private set; }
        public string DisplayName { get; private set; }
        public UserRole Role { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsDisabled { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public string? EmailVerificationTokenHash { get; private set; }
        public DateTime? EmailVerificationExpiresAtUtc { get; private set; }
        public string? PasswordResetTokenHash { get; private set; }
        public DateTime? PasswordResetExpiresAtUtc { get; private set; }
        public DateTime? PasswordResetUsedAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? LastLoginAtUtc { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        
        private User(){}
        
        public User(string email, string displayName, UserRole role, string passwordHash, DateTime createdAtUtc)
        {
            Email = email;
            DisplayName = displayName;
            Role = role;
            PasswordHash = passwordHash;
            CreatedAtUtc = createdAtUtc;
            EmailConfirmed = false;
            IsDisabled = false;
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public void Disable()
        {
            IsDisabled = true;
        }

        public void Enable()
        {
            IsDisabled = false;
        }

        public void MarkLoggedIn(DateTime loggedInAtUtc)
        {
            LastLoginAtUtc = loggedInAtUtc;
        }

        public void SetEmailVerificationToken(string tokenHash, DateTime expiresAtUtc)
        {
            EmailVerificationTokenHash = tokenHash;
            EmailVerificationExpiresAtUtc = expiresAtUtc;
        }

        public void ConfirmEmail()
        {
            EmailConfirmed = true;
            EmailVerificationTokenHash = null;
            EmailVerificationExpiresAtUtc = null;
        }

        public void SetPasswordResetToken(string tokenHash, DateTime expiresAtUtc)
        {
            PasswordResetTokenHash = tokenHash;
            PasswordResetExpiresAtUtc = expiresAtUtc;
            PasswordResetUsedAtUtc = null;
        }

        public void MarkPasswordResetUsed(DateTime usedAtUtc)
        {
            PasswordResetUsedAtUtc = usedAtUtc;
            PasswordResetTokenHash = null;
            PasswordResetExpiresAtUtc = null;
        }
    }
}
