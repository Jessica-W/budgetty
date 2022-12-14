using Budgetty.Domain;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services
{
    public class FinancialStateService : IFinancialStateService
    {
        private readonly IFinancialsSnapshotManager _financialsSnapshotManager;
        private readonly IBudgetaryRepository _budgetaryRepository;
        private readonly IEventProcessor _eventProcessor;
        private readonly IDateTimeProvider _dateTimeProvider;

        public FinancialStateService(IFinancialsSnapshotManager financialsSnapshotManager, IBudgetaryRepository budgetaryRepository, IEventProcessor eventProcessor, IDateTimeProvider dateTimeProvider)
        {
            _financialsSnapshotManager = financialsSnapshotManager;
            _budgetaryRepository = budgetaryRepository;
            _eventProcessor = eventProcessor;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<FinancialState> GetCurrentFinancialStateForUserAsync(string userId)
        {
            var financialsSnapshot = await _financialsSnapshotManager.GetSnapshotAsync(userId);
            var now = _dateTimeProvider.GetDateNow();
            var allEvents = _budgetaryRepository.GetBudgetaryEventsForUser(userId, financialsSnapshot?.Date, now).ToList();
            var pools = _budgetaryRepository.GetBudgetaryPoolsForUser(userId, includeBankAccounts: true, includeBudgetaryEvents: false).ToList();
            var financialState = _eventProcessor.ProcessEvents(allEvents, financialsSnapshot, pools);

            return financialState;
        }
    }
}