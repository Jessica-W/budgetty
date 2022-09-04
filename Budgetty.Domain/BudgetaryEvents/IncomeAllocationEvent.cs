namespace Budgetty.Domain.BudgetaryEvents
{
    public class IncomeAllocationEvent : BudgetaryEvent
    {
        public BudgetaryPool Pool { get; set; } = null!;
        public int AmountInPennies { get; set; }

        protected override string DebugString()
        {
            return $"Income allocation of £{AmountInPennies / 100m:0.00} to {Pool.Name}";
        }
    }
}