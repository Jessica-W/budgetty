using Budgetty.Mvc.Models.Pools;
using Budgetty.Persistance.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class PoolsController : BaseController
    {
        private readonly IBudgetaryRepository _budgetaryRepository;

        public PoolsController(UserManager<IdentityUser> userManager, IBudgetaryRepository budgetaryRepository) : base(userManager)
        {
            _budgetaryRepository = budgetaryRepository;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();

            var model = new PoolsViewModel
            {
                Pools = _budgetaryRepository
                    .GetBudgetaryPoolsForUser(userId, includeBankAccounts: true, includeBudgetaryEvents: true)
                    .Select(x => new PoolViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            BankAccountName = x.BankAccount?.Name ?? "N/A",
                            Deletable = x.BudgetaryEventsAsDestination.Count + x.BudgetaryEventsAsSource.Count == 0,
                        }
                    )
                    .ToList(),
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int poolId)
        {
            _budgetaryRepository.DeletePool(GetUserId(), poolId);
            _budgetaryRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
