namespace Budgetty.Mvc.Models.Summary
{
    public class SummaryViewModel
    {
        public DateOnly CurrentDate { get; set; }
        public List<BankAccountViewModel> BankAccounts { get; set; } = new();
        public List<(string PoolName, decimal Balance)> DebtPoolBalances { get; set; } = new();
        public decimal UnallocatedIncome { get; set; }
    }

    public class BankAccountViewModel
    {
        public int Id { get; set; }
        public string AccountName { get; set; } = "";
        public decimal AccountBalance { get; set; }
        public List<(string PoolName, decimal Balance)> IncomePoolBalances { get; set; } = new();
    }
}