using Budgetty.Mvc.Models.Pools;
using Budgetty.Persistance.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class PoolsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IBudgetaryRepository _budgetaryRepository;

        public PoolsController(UserManager<IdentityUser> userManager, IBudgetaryRepository budgetaryRepository)
        {
            _userManager = userManager;
            _budgetaryRepository = budgetaryRepository;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var model = new PoolsViewModel
            {
                Pools = _budgetaryRepository
                    .GetBudgetaryPoolsForUser(userId, includeBankAccounts: false)
                    .Select(x => x.Name)
                    .ToList(),
            };

            return View(model);
        }
    }
}
