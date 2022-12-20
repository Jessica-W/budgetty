namespace Budgetty.Mvc.Models.Transactions
{
    public class TransactionsViewModel
    {
        public DateOnly EarliestTransaction { get; set; }
        public DateOnly LatestTransaction { get; set; }
        public DateOnly? TransactionsStartDate { get; set; }
        public DateOnly? TransactionsEndDate { get; set; }
        public List<TransactionViewModel> Transactions { get; set; } = new();
    }

    public class TransactionViewModel
    {
        public DateOnly Date { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}