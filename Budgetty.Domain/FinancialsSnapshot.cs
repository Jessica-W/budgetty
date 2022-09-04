using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Domain
{
    public class FinancialsSnapshot
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int UnallocatedIncomeInPennies { get; set; }
        public List<PoolSnapshot> PoolSnapshots { get; set; } = new();
        public List<BankAccountSnapShot> BankAccountSnapShots { get; set; } = new();
        public List<BudgetaryEvent> BudgetaryEvents { get; set; } = new();
    }

    public class BankAccountSnapShot
    {
        public int Id { get; set; }
        public BankAccount BankAccount { get; set; } = new();
        public int BalanceInPennies { get; set; }
    }

    public class PoolSnapshot
    {
        public int Id { get; set; }
        public BudgetaryPool Pool { get; set; } = new();
        public int BalanceInPennies { get; set; }
    }
}