namespace Budgetty.Domain.BudgetaryEvents
{
    public class IncomeEvent : BudgetaryEvent
    {
        public int AmountInPennies { get; set; }
        public BudgetaryPool? DebtPool { get; set; }

        protected override string GetDescription()
        {
            return DebtPool != null
                ? $"Income created through borrowing {AmountInPennies / 100m:C} from {DebtPool.Name}"
                : $"Income of {AmountInPennies / 100m:C}";
        }
    }
}