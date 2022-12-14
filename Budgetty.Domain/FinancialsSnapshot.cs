using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain
{
    [ExcludeFromCodeCoverage]
    public class FinancialsSnapshot
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int UnallocatedIncomeInPennies { get; set; }
        public List<PoolSnapshot> PoolSnapshots { get; set; } = new();
        public List<BankAccountSnapShot> BankAccountSnapShots { get; set; } = new();
        public string? UserId { get; set; }
    }
}