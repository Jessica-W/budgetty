namespace Budgetty.Domain.BudgetaryEvents
{
    public class PoolTransferEvent : BudgetaryEvent
    {
        public BudgetaryPool SourcePool { get; set; } = new();
        public BudgetaryPool DestinationPool { get; set; } = new();
        public int AmountInPennies { get; set; }

        protected override string GetDescription()
        {
            return $"Transfer of {AmountInPennies / 100m:C} from {SourcePool.Name} to {DestinationPool.Name}";
        }
    }
}
