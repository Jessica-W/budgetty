using Budgetty.Domain;

namespace Budgetty.Services;

public interface IFinancialStateService
{
    Task<FinancialState> GetCurrentFinancialStateForUserAsync(string userId);
}