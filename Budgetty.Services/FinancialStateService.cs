using Budgetty.Domain;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services
{
    internal class FinancialStateService : IFinancialStateService
    {
        private readonly IFinancialsSnapshotManager _financialsSnapshotManager;
        private readonly IBudgetaryRepository _budgetaryRepository;
        private readonly IEventProcessor _eventProcessor;

        public FinancialStateService(IFinancialsSnapshotManager financialsSnapshotManager, IBudgetaryRepository budgetaryRepository, IEventProcessor eventProcessor)
        {
            _financialsSnapshotManager = financialsSnapshotManager;
            _budgetaryRepository = budgetaryRepository;
            _eventProcessor = eventProcessor;
        }

        public async Task<FinancialState> GetCurrentFinancialStateForUserAsync(string userId)
        {
            var financialsSnapshot = await _financialsSnapshotManager.GetSnapshotAsync(userId);
            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var allEvents = _budgetaryRepository.GetBudgetaryEventsForUser(userId, financialsSnapshot?.Date, now).ToList();
            var pools = _budgetaryRepository.GetBudgetaryPoolsForUser(userId, includeBankAccounts: true).ToList();
            var financialState = _eventProcessor.ProcessEvents(allEvents, financialsSnapshot, pools);

            return financialState;
        }
    }
}