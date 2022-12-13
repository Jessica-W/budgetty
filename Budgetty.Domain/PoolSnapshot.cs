using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain;

[ExcludeFromCodeCoverage]
public class PoolSnapshot
{
    public int Id { get; set; }
    public BudgetaryPool Pool { get; set; } = new();
    public int BalanceInPennies { get; set; }
}