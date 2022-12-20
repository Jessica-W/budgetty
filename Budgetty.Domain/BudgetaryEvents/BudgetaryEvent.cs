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
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Used as debt pool for income events, source pool for expenditure events and source pool for pool transfer events
        /// </summary>
        public BudgetaryPool? SourcePool { get; set; }

        /// <summary>
        /// Used for income allocation event and as destination in pool transfer event
        /// </summary>
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