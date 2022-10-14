namespace Budgetty.Domain
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }
}
