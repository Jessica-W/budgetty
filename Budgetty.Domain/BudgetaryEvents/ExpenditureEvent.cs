namespace Budgetty.Domain.BudgetaryEvents
{
    public class ExpenditureEvent : BudgetaryEvent
    {
        public BudgetaryPool Pool { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public int AmountInPennies { get; set; }

        protected override string DebugString()
        {
            return $"Expenditure of £{AmountInPennies / 100m:0.00} from {Pool.Name} (\"{Description}\")";
        }
    }
}