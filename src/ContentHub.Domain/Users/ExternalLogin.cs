using ContentHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentHub.Domain.Users
{
    public class ExternalLogin : Entity
    {
        public Guid UserId { get; private set; }
        public string Provider { get; private set; }
        public string ProviderUserId { get; private set; }
        public string? Email { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public ExternalLogin() { }

        public ExternalLogin(Guid userId, string provider, string providerUserId, string? email, DateTime createdAtUtc)
        {
            UserId = userId;
            Provider = provider;
            ProviderUserId = providerUserId;
            Email = email;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
