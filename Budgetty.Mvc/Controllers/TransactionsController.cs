using Budgetty.Mvc.Models.Transactions;
using Budgetty.Persistance.Repositories;
using Budgetty.Services;
using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly IBudgetaryRepository _budgetaryRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TransactionsController(UserManager<IdentityUser> userManager, IBudgetaryRepository budgetaryRepository, IDateTimeProvider dateTimeProvider) : base(userManager)
        {
            _budgetaryRepository = budgetaryRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        [HttpGet, HttpPost]
        public IActionResult Index(DateOnly? transactionsStartDate = null, DateOnly? transactionsEndDate = null)
        {
            if (transactionsStartDate.HasValue &&
                transactionsEndDate.HasValue &&
                (transactionsStartDate > transactionsEndDate ||
                    DateHelper.DaysBetween(transactionsStartDate.Value, transactionsEndDate.Value) > 90))
            {
                transactionsStartDate = null;
                transactionsEndDate = null;
            }

            var userId = GetUserId();
            var currentMonthStart = _dateTimeProvider.GetDateNow().MonthStart();
            var dateOfEarliestBudgetaryEvent = _budgetaryRepository.GetDateOfEarliestBudgetaryEventForUser(userId);
            var dateOfLatestBudgetaryEvent = _budgetaryRepository.GetDateOfLatestBudgetaryEventForUser(userId);

            transactionsStartDate ??= dateOfEarliestBudgetaryEvent.HasValue
                ? DateHelper.Max(dateOfEarliestBudgetaryEvent.Value.MonthStart(), currentMonthStart)
                : currentMonthStart;

            transactionsEndDate ??= transactionsStartDate.Value.MonthEnd();

            var budgetaryEvents = _budgetaryRepository.GetBudgetaryEventsForUser(userId, transactionsStartDate, transactionsEndDate).ToList();

            var viewModel = new TransactionsViewModel
            {
                EarliestTransaction = dateOfEarliestBudgetaryEvent ?? DateOnly.MinValue,
                LatestTransaction = dateOfLatestBudgetaryEvent ?? DateOnly.MaxValue,
                TransactionsStartDate = transactionsStartDate,
                TransactionsEndDate = transactionsEndDate,
            };

            viewModel.Transactions = budgetaryEvents
                .Select(x => new TransactionViewModel
                {
                    Date = x.Date,
                    Description = x.ToString(),
                })
                .ToList();
            
            return View(viewModel);
        }
    }
}
