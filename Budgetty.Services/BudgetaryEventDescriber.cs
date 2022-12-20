using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services
{
    public class BudgetaryEventDescriber : IBudgetaryEventDescriber
    {
        /// <summary>
        /// Returns a description of the event.
        /// </summary>
        /// <param name="budgetaryEvent">The <see cref="BudgetaryEvent"/>. Note that associated budgetary pool(s) must be loaded for this method to work.</param>
        /// <returns>A description of the event.</returns>
        /// <exception cref="ArgumentException">Budgetary event has invalid type.</exception>
        public string DescribeEvent(BudgetaryEvent budgetaryEvent)
        {
            return budgetaryEvent.EventType switch
            {
                BudgetaryEvent.BudgetaryEventType.Income => DescribeIncomeEvent(budgetaryEvent),
                BudgetaryEvent.BudgetaryEventType.IncomeAllocation => DescribeIncomeAllocationEvent(budgetaryEvent),
                BudgetaryEvent.BudgetaryEventType.Expenditure => DescribeExpenditureEvent(budgetaryEvent),
                BudgetaryEvent.BudgetaryEventType.PoolTransfer => DescribePoolTransferEvent(budgetaryEvent),
                _ => throw new ArgumentException("Unknown budget type", nameof(budgetaryEvent)),
            };
        }

        private static string DescribeIncomeEvent(BudgetaryEvent budgetaryEvent)
        {
            return budgetaryEvent.SourcePool != null
                ? $"Borrowed £{budgetaryEvent.AmountInPennies / 100m:0.00} from {budgetaryEvent.SourcePool.Name}"
                : $"Income of £{budgetaryEvent.AmountInPennies / 100m:0.00}";
        }

        private static string DescribeIncomeAllocationEvent(BudgetaryEvent budgetaryEvent)
        {
            return $"£{budgetaryEvent.AmountInPennies / 100m:0.00} of income allocated to {budgetaryEvent.DestinationPool!.Name}";
        }

        private static string DescribeExpenditureEvent(BudgetaryEvent budgetaryEvent)
        {
            return $"Expenditure of £{budgetaryEvent.AmountInPennies / 100m:0.00} from {budgetaryEvent.SourcePool!.Name}";
        }

        private static string DescribePoolTransferEvent(BudgetaryEvent budgetaryEvent)
        {
            return $"Transfer of £{budgetaryEvent.AmountInPennies / 100m:0.00} from {budgetaryEvent.SourcePool!.Name} to {budgetaryEvent.DestinationPool!.Name}";
        }
    }
}