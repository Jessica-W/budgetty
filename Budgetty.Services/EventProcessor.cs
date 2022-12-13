using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services;

public class EventProcessor : IEventProcessor
{
    public FinancialState ProcessEvents(List<BudgetaryEvent> allEvents, FinancialsSnapshot? financialsSnapshot,
        List<BudgetaryPool> pools, Action<BudgetaryEvent, FinancialState>? callback = null)
    {
        var bankAccounts = pools.Where(x => x.BankAccount != null)
            .Select(x => x.BankAccount!).ToList();
        var financialState = new FinancialState(pools, bankAccounts, financialsSnapshot);

        var orderedEvents = allEvents
            .OrderBy(x => x.Date)
            .ThenBy(x => x.SequenceNumber)
            .ToList();

        foreach (var budgetaryEvent in orderedEvents)
        {
            ProcessEvent(budgetaryEvent, financialState);
            callback?.Invoke(budgetaryEvent, financialState);
        }

        return financialState;
    }

    private static void ProcessEvent(BudgetaryEvent budgetaryEvent, FinancialState financialState)
    {
        switch (budgetaryEvent.EventType)
        {
            case BudgetaryEvent.BudgetaryEventType.Expenditure:
                ProcessExpenditureEvent(budgetaryEvent, financialState);
                break;

            case BudgetaryEvent.BudgetaryEventType.IncomeAllocation:
                ProcessIncomeAllocationEvent(budgetaryEvent, financialState);
                break;

            case BudgetaryEvent.BudgetaryEventType.Income:
                ProcessIncomeEvent(budgetaryEvent, financialState);
                break;

            case BudgetaryEvent.BudgetaryEventType.PoolTransfer:
                ProcessPoolTransferEvent(budgetaryEvent, financialState);
                break;

            default:
                throw new ArgumentException($"Unexpected event type ({budgetaryEvent.EventType})", nameof(budgetaryEvent));
        }
    }

    private static void ProcessIncomeEvent(BudgetaryEvent incomeEvent, FinancialState financialState)
    {
        financialState.UnallocatedIncomeInPennies += incomeEvent.AmountInPennies;

        if (incomeEvent.SourcePool != null)
        {
            if (incomeEvent.SourcePool.Type != PoolType.Debt)
            {
                throw new ArgumentException("Source pool must be a debt pool", nameof(incomeEvent));
            }

            financialState.AdjustPoolBalance(incomeEvent.SourcePool, incomeEvent.AmountInPennies);
        }
    }

    private static void ProcessIncomeAllocationEvent(BudgetaryEvent incomeAllocationEvent, FinancialState financialState)
    {
        var pool = incomeAllocationEvent.DestinationPool;

        financialState.UnallocatedIncomeInPennies -= incomeAllocationEvent.AmountInPennies;

        if (financialState.UnallocatedIncomeInPennies < 0)
        {
            throw new InvalidOperationException("Income allocation greater than unallocated income");
        }

        if (pool == null)
        {
            throw new ArgumentException("Income allocation event has no destination pool",
                nameof(incomeAllocationEvent));
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

    private static void ProcessPoolTransferEvent(BudgetaryEvent poolTransferEvent, FinancialState financialState)
    {
        var srcPool = poolTransferEvent.SourcePool;
        var dstPool = poolTransferEvent.DestinationPool;
        var amountInPennies = poolTransferEvent.AmountInPennies;

        if (srcPool == null || dstPool == null)
        {
            throw new ArgumentException("Source and destination pools must be provided", nameof(poolTransferEvent));
        }

        if (financialState.GetPoolBalance(srcPool) < amountInPennies)
        {
            throw new InvalidOperationException($"Amount in source pool \"{srcPool.Name}\" is less than the transfer amount of £{amountInPennies/100m:0.00}");
        }

        financialState.AdjustPoolBalance(srcPool, -amountInPennies);
        financialState.AdjustPoolBalance(dstPool, amountInPennies);
    }

    private static void ProcessExpenditureEvent(BudgetaryEvent expenditureEvent, FinancialState financialState)
    {
        var pool = expenditureEvent.SourcePool;

        if (pool == null)
        {
            throw new ArgumentException("Source pool cannot be null", nameof(expenditureEvent));
        }

        if (pool.Type == PoolType.Debt)
        {
            throw new ArgumentException($"Unable to expend from \"{pool.Name}\". Expenditures from a debt pools are disallowed.", nameof(expenditureEvent));
        }

        if (financialState.GetPoolBalance(pool) < expenditureEvent.AmountInPennies)
        {
            throw new InvalidOperationException($"Expenditure of £{expenditureEvent.AmountInPennies/100m:0.00} greater than balance in pool \"{pool.Name}\"");
        }

        financialState.AdjustPoolBalance(pool, -expenditureEvent.AmountInPennies);
    }
}