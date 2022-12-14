using System.Diagnostics.CodeAnalysis;
using Budgetty.Domain;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Persistance
{
    [ExcludeFromCodeCoverage] // Impossible to unit test
    internal class SnapshotLockManager : ISnapshotLockManager
    {
        private readonly BudgettyDbContext _budgettyDbContext;
        private const int MaxAttempts = 10;

        public SnapshotLockManager(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public async Task InitialiseLock(string userId)
        {
            var snapshotLock = new SnapshotLock
            {
                UserId = userId,
            };

            _budgettyDbContext.SnapshotLocks.Add(snapshotLock);
            await _budgettyDbContext.SaveChangesAsync();
        }

        public async Task<bool> TryGetLockAsync(string userId)
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var t1 = await _budgettyDbContext.Database.BeginTransactionAsync();
                var sl = (await _budgettyDbContext.SnapshotLocks
                    .FromSqlInterpolated($"SELECT * FROM SnapshotLocks WHERE UserId={userId} AND LockedAt is NULL FOR UPDATE")
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

        public async Task ReleaseLockAsync(string userId)
        {
            var t1 = await _budgettyDbContext.Database.BeginTransactionAsync();
            var sl = (await _budgettyDbContext.SnapshotLocks.FromSqlInterpolated($"SELECT * FROM SnapshotLocks WHERE UserId={userId} FOR UPDATE").ToListAsync()).First();
            sl.LockedAt = null;
            await _budgettyDbContext.SaveChangesAsync();
            await t1.CommitAsync();
        }
    }
}