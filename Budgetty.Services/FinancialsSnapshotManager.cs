﻿using Budgetty.Domain;
using Budgetty.Persistance;
using Budgetty.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Services
{
    public class FinancialsSnapshotManager : IFinancialsSnapshotManager
    {
        private readonly BudgettyDbContext _budgettyDbContext;
        private readonly ISnapshotLockManager _snapshotLockManager;

        public FinancialsSnapshotManager(BudgettyDbContext budgettyDbContext, ISnapshotLockManager snapshotLockManager)
        {
            _budgettyDbContext = budgettyDbContext;
            _snapshotLockManager = snapshotLockManager;
        }

        public async Task CreateSnapshotAsync(string userId, FinancialState state, DateOnly date)
        {
            var snapshotId = Guid.NewGuid();

            var snapshot = new FinancialsSnapshot
            {
                Id = snapshotId,
                UnallocatedIncomeInPennies = state.UnallocatedIncomeInPennies,
                Date = date,
                PoolSnapshots = state.GetPoolBalancesInPennies().Select(x => new PoolSnapshot
                {
                    Pool = x.Pool,
                    BalanceInPennies = x.BalanceInPennies,
                }).ToList(),
                BankAccountSnapShots = state.GetBankAccountBalancesInPennies().Select(x => new BankAccountSnapShot
                {
                    BankAccount = x.BankAccount,
                    BalanceInPennies = x.BalanceInPennies,
                }).ToList(),
            };

            var gotLock = await _snapshotLockManager.TryGetLockAsync(userId);

            if (gotLock)
            {
                try
                {
                    _budgettyDbContext.FinancialsSnapshots.Add(snapshot);
                    await _budgettyDbContext.SaveChangesAsync();
                }
                finally
                {
                    await _snapshotLockManager.ReleaseLockAsync(userId);
                }
            }
        }

        public async Task<FinancialsSnapshot?> GetSnapshotAsync(string userId)
        {
            var financialsSnapshots = await _budgettyDbContext.FinancialsSnapshots
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var financialsSnapshot = financialsSnapshots.FirstOrDefault();

            if (financialsSnapshot == null)
            {
                return null;
            }

            return await _budgettyDbContext.FinancialsSnapshots
                .Where(x => x.Id == financialsSnapshot.Id)
                .Include(x => x.BankAccountSnapShots)
                .ThenInclude(x => x.BankAccount)
                .Include(x => x.PoolSnapshots)
                .ThenInclude(x => x.Pool)
                .SingleAsync();
        }
    }
}