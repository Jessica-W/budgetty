using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain;

[ExcludeFromCodeCoverage]
public class BankAccountSnapShot
{
    public int Id { get; set; }
    public BankAccount BankAccount { get; set; } = new();
    public int BalanceInPennies { get; set; }
}