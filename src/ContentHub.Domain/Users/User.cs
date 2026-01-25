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
        
        private User(){}
        
        public User(string email, string displayName, UserRole role, string passwordHash)
        {
            Email = email;
            DisplayName = displayName;
            Role = role;
            PasswordHash = passwordHash;
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }
    }
}
