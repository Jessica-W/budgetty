using Budgetty.Persistance;
using Budgetty.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Services
{
    public class SnapshotLockManager : ISnapshotLockManager
    {
        private readonly BudgettyDbContext _budgettyDbContext;
        private const int MaxAttempts = 10;

        public SnapshotLockManager(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public async Task<bool> TryGetLockAsync()
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var t1 = await _budgettyDbContext.Database.BeginTransactionAsync();
                var sl = (await _budgettyDbContext.SnapshotLocks
                    .FromSqlRaw("SELECT * FROM SnapshotLocks WHERE Id=1 AND LockedAt is NULL FOR UPDATE")
                    .ToListAsync()).FirstOrDefault();

                if (sl != null)
                {
                    sl.LockedAt = DateTime.UtcNow;
                    await _budgettyDbContext.SaveChangesAsync();
                    await t1.CommitAsync();

                    return true;
                }

                await t1.RollbackAsync();
                await Task.Delay(100);
            }

            return false;
        }

        public async Task ReleaseLockAsync()
        {
            var t1 = await _budgettyDbContext.Database.BeginTransactionAsync();
            var sl = (await _budgettyDbContext.SnapshotLocks.FromSqlRaw("SELECT * FROM SnapshotLocks WHERE Id=1 FOR UPDATE").ToListAsync()).First();
            sl.LockedAt = null;
            await _budgettyDbContext.SaveChangesAsync();
            await t1.CommitAsync();
        }
    }
}