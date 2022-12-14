using Budgetty.Domain;
using Budgetty.Persistance;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services
{
    public class FinancialsSnapshotManager : IFinancialsSnapshotManager
    {
        private readonly ISnapshotLockManager _snapshotLockManager;

        public FinancialsSnapshotManager(ISnapshotLockManager snapshotLockManager)
        {
            _snapshotLockManager = snapshotLockManager;
        }

        public Task CreateSnapshotAsync(string userId, FinancialState state, DateOnly date)
        {
            // TODO: Use repository to save snapshot

            return Task.CompletedTask;
        }

        public Task<FinancialsSnapshot?> GetSnapshotAsync(string userId)
        {
            //TODO: use repository to get snapshots

            return Task.FromResult<FinancialsSnapshot?>(null);
        }
    }
}