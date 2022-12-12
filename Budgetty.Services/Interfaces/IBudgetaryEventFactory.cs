using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Services.Interfaces;

public interface IBudgetaryEventFactory
{
    BudgetaryEvent CreateIncomeEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool? debtPool = null);
    BudgetaryEvent CreateIncomeAllocationEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool pool);
    BudgetaryEvent CreateExpenditureEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool pool);
    BudgetaryEvent CreatePoolTransferEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool sourcePool, BudgetaryPool destinationPool);
}