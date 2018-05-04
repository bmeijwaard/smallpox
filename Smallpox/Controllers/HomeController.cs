using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smallpox.Controllers.Base;
using Smallpox.Models;
using Smallpox.Services;
using System.Threading.Tasks;

namespace Smallpox.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var model = new UserViewModel();
            var user = await _userService.GetAsync(new System.Guid("E68E0F0A-D04F-E811-9863-4CCC6AFD505C"));
            if (user.Succeeded)
            {
                model.User = user.Entity;
            }
            return View(model);
        }
    }
}
