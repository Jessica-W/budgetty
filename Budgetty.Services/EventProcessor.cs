using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services;

public class EventProcessor : IEventProcessor
{
    public FinancialState ProcessEvents(List<BudgetaryEvent> allEvents, FinancialsSnapshot? financialsSnapshot,
        List<BudgetaryPool> pools, Action<BudgetaryEvent, FinancialState>? callback = null)
    {
        var bankAccounts = pools.Where(x => x.BankAccount != null).Select(x => x.BankAccount!).ToList();
        var financialState = new FinancialState(pools, bankAccounts, financialsSnapshot);

        var orderedEvents = allEvents.OrderBy(x => x.SequenceNumber).ToList();

        foreach (dynamic budgetaryEvent in orderedEvents)
        {
            ProcessEvent(budgetaryEvent, financialState);

            if (callback != null)
            {
                callback(budgetaryEvent, financialState);
            }
        }

        return financialState;
    }

    // ReSharper disable once UnusedParameter.Local
    // ReSharper disable once EntityNameCapturedOnly.Local
    private void ProcessEvent(BudgetaryEvent budgetaryEvent, FinancialState _)
    {
        throw new ArgumentException("Unexpected budgetary event type", nameof(budgetaryEvent));
    }

    private void ProcessEvent(IncomeEvent incomeEvent, FinancialState financialState)
    {
        financialState.UnallocatedIncomeInPennies += incomeEvent.AmountInPennies;

        if (incomeEvent.DebtPool != null)
        {
            financialState.AdjustPoolBalance(incomeEvent.DebtPool, incomeEvent.AmountInPennies);
        }
    }

    private void ProcessEvent(IncomeAllocationEvent incomeAllocationEvent, FinancialState financialState)
    {
        var pool = incomeAllocationEvent.Pool;

        financialState.UnallocatedIncomeInPennies -= incomeAllocationEvent.AmountInPennies;

        if (financialState.UnallocatedIncomeInPennies < 0)
        {
            throw new InvalidOperationException("Income allocation greater than unallocated income");
        }

        if (pool.Type == PoolType.Debt)
        {
            if (financialState.GetPoolBalance(pool) - incomeAllocationEvent.AmountInPennies < 0)
            {
                throw new InvalidOperationException("Income allocation greater than debt amount");
            }

            financialState.AdjustPoolBalance(pool, -incomeAllocationEvent.AmountInPennies);
        }
        else
        {
            financialState.AdjustPoolBalance(pool, incomeAllocationEvent.AmountInPennies);
        }
    }

    private void ProcessEvent(PoolTransferEvent poolTransferEvent, FinancialState financialState)
    {
        var srcPool = poolTransferEvent.SourcePool;
        var dstPool = poolTransferEvent.DestinationPool;
        var amountInPennies = poolTransferEvent.AmountInPennies;

        if (financialState.GetPoolBalance(srcPool) < amountInPennies)
        {
            throw new InvalidOperationException($"Amount in source pool \"{srcPool.Name}\" is less than the transfer amount of £{amountInPennies/100m:0.00}");
        }

        financialState.AdjustPoolBalance(srcPool, -amountInPennies);
        financialState.AdjustPoolBalance(dstPool, amountInPennies);
    }

    private void ProcessEvent(ExpenditureEvent expenditureEvent, FinancialState financialState)
    {
        if (expenditureEvent.Pool.Type == PoolType.Debt)
        {
            throw new InvalidOperationException($"Unable to expend from \"{expenditureEvent.Pool.Name}\". Expenditures from a debt pools are disallowed.");
        }

        if (financialState.GetPoolBalance(expenditureEvent.Pool) < expenditureEvent.AmountInPennies)
        {
            throw new InvalidOperationException($"Expenditure of £{expenditureEvent.AmountInPennies/100m:0.00} greater than balance in pool \"{expenditureEvent.Pool.Name}\"");
        }

        financialState.AdjustPoolBalance(expenditureEvent.Pool, -expenditureEvent.AmountInPennies);
    }
}