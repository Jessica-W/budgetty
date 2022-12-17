using Budgetty.Mvc.Models.BankAccounts;
using Budgetty.Persistance.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class BankAccountsController : BaseController
    {
        private readonly IBudgetaryRepository _budgetaryRepository;

        public BankAccountsController(UserManager<IdentityUser> userManager, IBudgetaryRepository budgetaryRepository) :
            base(userManager)
        {
            _budgetaryRepository = budgetaryRepository;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();

            var bankAccounts = _budgetaryRepository.GetBankAccountsForUser(userId, includeBudgetaryPools: true);
            var viewModel = new BankAccountsViewModel
            {
                BankAccounts = bankAccounts.Select(x => new BankAccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Deletable = !x.BudgetaryPools.Any(),
                }).ToList(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int bankAccountId)
        {
            _budgetaryRepository.DeleteBankAccount(GetUserId(), bankAccountId);
            _budgetaryRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBankAccount(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must be provided", nameof(name));
            }

            _budgetaryRepository.CreateBankAccount(GetUserId(), name);
            _budgetaryRepository.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}