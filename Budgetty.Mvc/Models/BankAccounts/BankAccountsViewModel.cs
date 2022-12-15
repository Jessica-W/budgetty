namespace Budgetty.Mvc.Models.BankAccounts
{
    public class BankAccountsViewModel
    {
        public List<BankAccountViewModel> BankAccounts { get; set; } = new();
    }

    public class BankAccountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}