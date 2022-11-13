namespace Budgetty.Domain.BudgetaryEvents
{
    public class IncomeAllocationEvent : BudgetaryEvent
    {
        public BudgetaryPool Pool { get; set; } = null!;
        public int AmountInPennies { get; set; }

        protected override string GetDescription()
        {
            return $"Income allocation of {AmountInPennies / 100m:C} to {Pool.Name}";
        }
    }
}