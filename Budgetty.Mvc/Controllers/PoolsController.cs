using Budgetty.Domain;
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
                            Type = x.Type.ToString(),
                            Deletable = x.BudgetaryEventsAsDestination.Count + x.BudgetaryEventsAsSource.Count == 0,
                        }
                    )
                    .OrderBy(x => x.BankAccountName)
                    .ToList(),
                AvailableBankAccounts = _budgetaryRepository.GetBankAccountsForUser(userId, includeBudgetaryPools: false)
                    .Select(x => new AvailableBankAccountViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                    })
                    .ToList(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int poolId)
        {
            _budgetaryRepository.DeletePool(GetUserId(), poolId);
            _budgetaryRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePool(string name, PoolType poolType, int bankAccountId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must be provided", nameof(name));
            }

            if (poolType != PoolType.Income && poolType != PoolType.Debt)
            {
                throw new ArgumentException("Invalid pool type", nameof(poolType));
            }

            var userId = GetUserId();
            var bankAccount = poolType == PoolType.Income ? _budgetaryRepository.GetBankAccountForUser(userId, bankAccountId) : null;

            if (poolType == PoolType.Income && bankAccount == null)
            {
                throw new ArgumentException("Invalid bank account ID", nameof(bankAccountId));
            }

            _budgetaryRepository.CreateBudgetaryPoolAccount(userId, name, poolType, bankAccount);
            _budgetaryRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
