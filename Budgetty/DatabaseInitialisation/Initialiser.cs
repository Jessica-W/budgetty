using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;

namespace Budgetty.DatabaseInitialisation
{
    public class Initialiser
    {
        private readonly BudgettyDbContext _budgettyDbContext;

        public Initialiser(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public void Initialise()
        {
            var natwestSavings = new BankAccount { Name = "Natwest Savings" };
            var natwestCurrent = new BankAccount { Name = "Natwest Savings" };

            _budgettyDbContext.BankAccounts.Add(natwestSavings);
            _budgettyDbContext.BankAccounts.Add(natwestCurrent);

            var currentPool = new BudgetaryPool { Type = PoolType.Income, BankAccount = natwestCurrent, Name = "Current" };
            var savingsPool = new BudgetaryPool { Type = PoolType.Income, BankAccount = natwestSavings, Name = "Savings" };
            var jessicaDebtPool = new BudgetaryPool { Type = PoolType.Debt, Name = "Debt to Jessica" };

            _budgettyDbContext.BudgetaryPools.Add(currentPool);
            _budgettyDbContext.BudgetaryPools.Add(savingsPool);
            _budgettyDbContext.BudgetaryPools.Add(jessicaDebtPool);

            _budgettyDbContext.BudgetaryEvents.Add(new IncomeEvent
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                AmountInPennies = 170_00,
                SequenceNumber = 1,
            });

            _budgettyDbContext.SequenceNumbers.Add(new SequenceNumber { SequenceNo = 2 });
            _budgettyDbContext.SnapshotLocks.Add(new SnapshotLock { Id = 1, LockedAt = null });

            _budgettyDbContext.SaveChanges();
        }
    }
}
