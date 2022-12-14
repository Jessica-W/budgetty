using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Persistance.Repositories
{
    [ExcludeFromCodeCoverage] // Impossible to unit test
    internal class BudgetaryRepository : IBudgetaryRepository
    {
        private readonly BudgettyDbContext _budgettyDbContext;

        public BudgetaryRepository(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public DateOnly? GetDateOfEarliestBudgetaryEventForUser(string userId)
        {
            var earliestEvent = _budgettyDbContext.BudgetaryEvents
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            return earliestEvent?.Date;
        }

        public DateOnly? GetDateOfLatestBudgetaryEventForUser(string userId)
        {
            var earliestEvent = _budgettyDbContext.BudgetaryEvents
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            return earliestEvent?.Date;
        }

        public IEnumerable<BudgetaryEvent> GetBudgetaryEventsForUser(string userId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _budgettyDbContext.BudgetaryEvents
                .Where(x => x.UserId == userId);

            if (startDate != null)
            {
                query = query.Where(x => x.Date >= startDate);
            }

            if (endDate != null)
            {
                query = query.Where(x => x.Date <= endDate);
            }

            return query
                .Include(x => x.SourcePool)
                .Include(x => x.DestinationPool)
                .OrderBy(x => x.SequenceNumber).AsEnumerable();
        }

        public IEnumerable<BudgetaryPool> GetBudgetaryPoolsForUser(string userId, bool includeBankAccounts, bool includeBudgetaryEvents)
        {
            var query = _budgettyDbContext.BudgetaryPools
                .Where(x => x.UserId == userId);

            if (includeBankAccounts)
            {
                query = query.Include(x => x.BankAccount);
            }

            if (includeBudgetaryEvents)
            {
                query = query
                    .Include(x => x.BudgetaryEventsAsDestination)
                    .Include(x => x.BudgetaryEventsAsSource);
            }

            return query.AsEnumerable();
        }

        public void AddBudgetaryPool(BudgetaryPool pool)
        {
            if (string.IsNullOrWhiteSpace(pool.UserId))
            {
                throw new ArgumentException($"{nameof(pool.UserId)} cannot be null");
            }

            _budgettyDbContext.Add(pool);
        }

        public void AddBudgetaryEvent(BudgetaryEvent budgetaryEvent)
        {
            if (string.IsNullOrWhiteSpace(budgetaryEvent.UserId))
            {
                throw new ArgumentException($"{nameof(budgetaryEvent.UserId)} cannot be null");
            }

            _budgettyDbContext.Add(budgetaryEvent);
        }

        public void SaveChanges()
        {
            _budgettyDbContext.SaveChanges();
        }

        public List<BankAccount> GetBankAccountsForUser(string userId)
        {
            return _budgettyDbContext
                .BankAccounts
                .Where(x => x.UserId == userId)
                .ToList();
        }

        public BankAccount? GetBankAccountForUser(string userId, int bankAccountId)
        {
            return _budgettyDbContext.BankAccounts.FirstOrDefault(x => x.UserId == userId && x.Id == bankAccountId);
        }

        public void CreateBudgetaryPoolAccount(string userId, string name, PoolType poolType, BankAccount? bankAccount)
        {
            _budgettyDbContext.BudgetaryPools.Add(new BudgetaryPool
            {
                Name = name,
                Type = poolType,
                BankAccount = bankAccount,
                UserId = userId,
            });
        }

        public void DeletePool(string userId, int poolId)
        {
            var pool = _budgettyDbContext.BudgetaryPools.FirstOrDefault(x => x.Id == poolId);

            if (pool != null)
            {
                if (pool.UserId != userId)
                {
                    throw new SecurityViolationException("Attempt to delete another user's budgetary pool");
                }

                _budgettyDbContext.BudgetaryPools.Remove(pool);
            }
        }
    }
}