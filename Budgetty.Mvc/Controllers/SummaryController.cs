using Budgetty.Mvc.Mappers;
using Budgetty.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Controllers
{
    [Authorize]
    public class SummaryController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFinancialStateService _financialStateService;
        private readonly IFinancialStateMapper _financialStateMapper;

        public SummaryController(UserManager<IdentityUser> userManager, IFinancialStateMapper financialStateMapper, IFinancialStateService financialStateService)
        {
            _userManager = userManager;
            _financialStateMapper = financialStateMapper;
            _financialStateService = financialStateService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var financialState = await _financialStateService.GetCurrentFinancialStateForUserAsync(userId);
            var viewModel = _financialStateMapper.MapToSummaryViewModel(financialState);

            return View(viewModel);
        }
    }
}
