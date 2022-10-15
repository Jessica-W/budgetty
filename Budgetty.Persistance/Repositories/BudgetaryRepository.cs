using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Persistance.Repositories
{
    internal class BudgetaryRepository : IBudgetaryRepository
    {
        private readonly BudgettyDbContext _budgettyDbContext;

        public BudgetaryRepository(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public IEnumerable<BudgetaryEvent> GetBudgetaryEventsForUser(string userId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _budgettyDbContext.BudgetaryEvents.Where(x => x.UserId == userId);

            if (startDate != null)
            {
                query = query.Where(x => x.Date >= startDate);
            }

            if (endDate != null)
            {
                query = query.Where(x => x.Date <= endDate);
            }

            return query.AsEnumerable();
        }

        public IEnumerable<BudgetaryPool> GetBudgetaryPoolsForUser(string userId, bool includeBankAccounts)
        {
            var query = _budgettyDbContext.BudgetaryPools.Where(x => x.UserId == userId);

            if (includeBankAccounts)
            {
                query = query.Include(x => x.BankAccount);
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
    }
}