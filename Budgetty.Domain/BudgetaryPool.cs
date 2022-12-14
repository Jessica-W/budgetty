using Budgetty.Domain.BudgetaryEvents;
using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain
{
    [ExcludeFromCodeCoverage]
    public class BudgetaryPool
    {
        private BankAccount? _bankAccount;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PoolType Type { get; set; }

        public BankAccount? BankAccount
        {
            get => _bankAccount;
            set
            {
                if (Type == PoolType.Income)
                {
                    _bankAccount = value;
                }
                else if (value != null)
                {
                    throw new InvalidOperationException("Cannot assign bank account to debt pool");
                }
            }
        }

        public string? UserId { get; set; }

        public List<BudgetaryEvent> BudgetaryEventsAsSource { get; set; } = new();
        public List<BudgetaryEvent> BudgetaryEventsAsDestination { get; set; } = new();
    }
}