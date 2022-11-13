using Budgetty.Domain;

namespace Budgetty.Services.Interfaces;

public interface IFinancialStateService
{
    Task<FinancialState> GetCurrentFinancialStateForUserAsync(string userId);
}