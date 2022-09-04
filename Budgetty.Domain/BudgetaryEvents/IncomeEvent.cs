namespace Budgetty.Domain.BudgetaryEvents
{
    public class IncomeEvent : BudgetaryEvent
    {
        public int AmountInPennies { get; set; }
        public BudgetaryPool? DebtPool { get; set; }

        protected override string DebugString()
        {
            return DebtPool != null
                ? $"Income created through borrowing {AmountInPennies / 100m:0.00} from {DebtPool.Name}"
                : $"Income of £{AmountInPennies / 100m:0.00}";
        }
    }
}