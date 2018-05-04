using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Smallpox.Controllers.Base
{
    [Authorize]
    public class BaseController : Controller
    {
    }
}
