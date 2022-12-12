using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain
{
    [ExcludeFromCodeCoverage]
    public class BankAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UserId { get; set; }

        public List<BudgetaryPool> BudgetaryPools { get; set; } = new();
    }
}
