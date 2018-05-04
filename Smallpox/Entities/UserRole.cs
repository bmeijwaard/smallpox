using Microsoft.AspNetCore.Identity;
using System;

namespace Smallpox.Entities
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public virtual Role Role { get; set; }
    }
}
