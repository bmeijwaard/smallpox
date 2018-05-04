using Microsoft.AspNetCore.Identity;
using Smallpox.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Smallpox.Services.Identity
{
    public interface IUserManager : IDisposable
    {
        Task<IdentityResult> CreateAsync(User user, string password);
        Task<User> FindByIdAsync(string userId);
        Task<IdentityResult> ConfirmEmailAsync(User user, string code);
        Task<User> FindByEmailAsync(string email);
        Task<User> FindByNameAsync(string userName);
        Task<bool> IsEmailConfirmedAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string code, string password);
        Task<IdentityResult> ChangePasswordAsync(User user, string modelOldPassword, string modelNewPassword);
        Task<IdentityResult> AddPasswordAsync(User user, string modelNewPassword);
        Task<IdentityResult> AddToRoleAsync(User user, string role);
        Task<IdentityResult> RemoveFromRoleAsync(User user, string role);
        Task<IList<string>> GetRolesAsync(User user);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<bool> IsLockedOutAsync(User user);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<IdentityResult> ResetAccessFailedCountAsync(User user);
        Task<IdentityResult> AccessFailedAsync(User user);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(User user);
        Task<User> GetUserAsync(ClaimsPrincipal principal);
        Task<IList<Claim>> GetClaimsAsync(User user);
        Task<IdentityResult> SetLockoutEnabledAsync(User user, bool enabled);
    }
}
