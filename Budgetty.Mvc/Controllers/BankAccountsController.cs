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

        public BankAccountsController(UserManager<IdentityUser> userManager, IBudgetaryRepository budgetaryRepository) : base(userManager)
        {
            _budgetaryRepository = budgetaryRepository;
        }

        public IActionResult Index()
        {
            var userId = GetUserId();

            var bankAccounts = _budgetaryRepository.GetBankAccountsForUser(userId);
            var viewModel = new BankAccountsViewModel
            {
                BankAccounts = bankAccounts.Select(x => new BankAccountViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToList(),
            };

            return View(viewModel);
        }
    }
}