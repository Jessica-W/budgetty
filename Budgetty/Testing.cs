using Budgetty.DatabaseInitialisation;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budgetty
{
    public class Testing
    {
        private readonly IEventProcessor _eventProcessor;
        private readonly BudgettyDbContext _budgettyDbContext;
        private readonly Initialiser _initialiser;
        private readonly IFinancialsSnapshotManager _financialsSnapshotManager;
        private readonly ISequenceNumberProvider _sequenceNumberProvider;

        public Testing(BudgettyDbContext budgettyDbContext, IEventProcessor eventProcessor, Initialiser initialiser, IFinancialsSnapshotManager financialsSnapshotManager, ISequenceNumberProvider sequenceNumberProvider)
        {
            _budgettyDbContext = budgettyDbContext;
            _eventProcessor = eventProcessor;
            _initialiser = initialiser;
            _financialsSnapshotManager = financialsSnapshotManager;
            _sequenceNumberProvider = sequenceNumberProvider;
        }

        public async Task Test()
        {
            if (!_budgettyDbContext.BudgetaryPools.Any())
            {
                _initialiser.Initialise();
            }

            var financialsSnapshot = await _financialsSnapshotManager.GetSnapshotAsync();

            var pools = _budgettyDbContext.BudgetaryPools
                .Include(x => x.BankAccount)
                .ToList();

            _budgettyDbContext.BudgetaryEvents.Add(new IncomeAllocationEvent
            {
                Date = new DateOnly(2022, 9, 5),
                AmountInPennies = 500,
                Pool = pools.First(x => x.Type != PoolType.Debt),
                SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(),
            });
            await _budgettyDbContext.SaveChangesAsync();

            var allEvents =
                _budgettyDbContext.BudgetaryEvents
                    .Where(x => financialsSnapshot == null || x.Date > financialsSnapshot.Date)
                    .ToList();

            FinancialState? financialState = null;

            for (int n = 0; n <= allEvents.Count; n++)
            {
                if (n > 0)
                {
                    var budgetaryEvent = allEvents[n - 1];

                    Console.WriteLine($"{budgetaryEvent}");
                }

                financialState = GetFinancials(allEvents.Take(n).ToList(), financialsSnapshot, pools);

                if (n == 0)
                {
                    financialState = null;
                }
            }

            if (financialState != null)
            {
                await _financialsSnapshotManager.CreateSnapshotAsync(financialState, allEvents.Last().Date);
            }
        }

        private FinancialState GetFinancials(List<BudgetaryEvent> allEvents, FinancialsSnapshot? financialsSnapshot,
            List<BudgetaryPool> pools)
        {
            var financialState = _eventProcessor.ProcessEvents(allEvents, financialsSnapshot, pools);

            foreach (var poolBalance in financialState.GetPoolBalancesInPennies())
            {
                Console.WriteLine($"{poolBalance.Pool.Name} = £{poolBalance.BalanceInPennies / 100m:0.00}");
            }

            Console.WriteLine($"Income unallocated £{financialState.UnallocatedIncomeInPennies / 100m:0.00}\n");

            return financialState;
        }
    }
}