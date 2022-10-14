using Budgetty.Mvc.Models.Pools;
using Budgetty.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    public class PoolsController : Controller
    {
        private readonly BudgettyDbContext _budgettyDbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public PoolsController(BudgettyDbContext budgettyDbContext, UserManager<IdentityUser> userManager)
        {
            _budgettyDbContext = budgettyDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var model = new PoolsViewModel
            {
                Pools = _budgettyDbContext.BudgetaryPools.Where(x => x.UserId == userId).Select(x => x.Name).ToList(),
            };

            return View(model);
        }
    }
}
