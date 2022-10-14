namespace Budgetty.Domain
{
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
                else
                {
                    throw new InvalidOperationException("Cannot assign bank account to debt pool");
                }
            }
        }

        public string? UserId { get; set; }
    }
}