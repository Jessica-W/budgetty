using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc
{
    public class BaseController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public BaseController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected string GetUserId()
        {
            return _userManager.GetUserId(User);
        }
    }
}