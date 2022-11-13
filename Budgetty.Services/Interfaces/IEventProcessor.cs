using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Services.Interfaces;

public interface IEventProcessor
{
    FinancialState ProcessEvents(List<BudgetaryEvent> allEvents, FinancialsSnapshot? financialsSnapshot,
        List<BudgetaryPool> pools, Action<BudgetaryEvent, FinancialState>? callback = null);
}