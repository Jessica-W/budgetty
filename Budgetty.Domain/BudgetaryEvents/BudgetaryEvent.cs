namespace Budgetty.Domain.BudgetaryEvents
{
    public abstract class BudgetaryEvent
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int SequenceNumber { get; set; }
        public string? UserId { get; set; }

        protected abstract string GetDescription();

        public override string ToString()
        {
            return GetDescription();
        }
    }
}
