using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Domain.BudgetaryEvents
{
    [ExcludeFromCodeCoverage]
    public class BudgetaryEvent
    {
        public int Id { get; set; }
        public BudgetaryEventType EventType { get; set; }
        public DateOnly Date { get; set; }
        public int SequenceNumber { get; set; }
        public string? UserId { get; set; }
        public int AmountInPennies { get; set; }
        public string Description { get; set; } = string.Empty;

        public BudgetaryPool? SourcePool { get; set; }
        public BudgetaryPool? DestinationPool { get; set; }

        public enum BudgetaryEventType
        {
            Expenditure,
            IncomeAllocation,
            Income,
            PoolTransfer,
        }
    }
}