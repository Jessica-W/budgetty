using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services
{
    public class BudgetaryEventFactory : IBudgetaryEventFactory
    {
        private readonly ISequenceNumberProvider _sequenceNumberProvider;

        public BudgetaryEventFactory(ISequenceNumberProvider sequenceNumberProvider)
        {
            _sequenceNumberProvider = sequenceNumberProvider;
        }

        public BudgetaryEvent CreateIncomeEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool? debtPool = null)
        {
            return new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Income,
                Date = date,
                UserId = userId,
                AmountInPennies = amountInPennies,
                SourcePool = debtPool,
                SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
            };
        }

        public BudgetaryEvent CreateIncomeAllocationEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool pool)
        {
            return new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                Date = date,
                UserId = userId,
                AmountInPennies = amountInPennies,
                DestinationPool = pool,
                SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
            };
        }

        public BudgetaryEvent CreateExpenditureEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool pool)
        {
            return new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                Date = date,
                UserId = userId,
                AmountInPennies = amountInPennies,
                SourcePool = pool,
                SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
            };
        }

        public BudgetaryEvent CreatePoolTransferEvent(DateOnly date, string userId, int amountInPennies, BudgetaryPool sourcePool, BudgetaryPool destinationPool)
        {
            return new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                Date = date,
                UserId = userId,
                AmountInPennies = amountInPennies,
                SourcePool = sourcePool,
                DestinationPool = destinationPool,
                SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
            };
        }
    }
}