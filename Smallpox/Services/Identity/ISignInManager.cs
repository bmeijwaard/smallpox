using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Smallpox.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Smallpox.Services.Identity
{
    public interface ISignInManager
    {
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool rememberMe, bool lockoutOnFailure);

        Task SignInAsync(User user, bool isPersistent);
        Task SignOutAsync();
        bool IsSignedIn(ClaimsPrincipal user);
        Task<bool> CanSignInAsync(User user);
        Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user);
        IHttpContextAccessor GetHttpContext();
    }
}
