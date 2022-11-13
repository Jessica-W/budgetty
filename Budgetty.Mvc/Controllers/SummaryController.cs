using Budgetty.Mvc.Mappers;
using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class SummaryController : BaseController
    {
        private readonly IFinancialStateService _financialStateService;
        private readonly IFinancialStateMapper _financialStateMapper;

        public SummaryController(UserManager<IdentityUser> userManager, IFinancialStateMapper financialStateMapper, IFinancialStateService financialStateService) : base(userManager)
        {
            _financialStateMapper = financialStateMapper;
            _financialStateService = financialStateService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var financialState = await _financialStateService.GetCurrentFinancialStateForUserAsync(userId);
            var viewModel = _financialStateMapper.MapToSummaryViewModel(financialState);

            return View(viewModel);
        }
    }
}
