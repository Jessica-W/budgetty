using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Persistance.Repositories;

public interface IBudgetaryRepository
{
    DateOnly? GetDateOfEarliestBudgetaryEventForUser(string userId);
    DateOnly? GetDateOfLatestBudgetaryEventForUser(string userId);
    IEnumerable<BudgetaryEvent> GetBudgetaryEventsForUser(string userId, DateOnly? startDate = null, DateOnly? endDate = null);
    IEnumerable<BudgetaryPool> GetBudgetaryPoolsForUser(string userId, bool includeBankAccounts, bool includeBudgetaryEvents);
    void AddBudgetaryPool(BudgetaryPool pool);
    void AddBudgetaryEvent(BudgetaryEvent budgetaryEvent);
    void DeletePool(string userId, int poolId);
    void SaveChanges();
    List<BankAccount> GetBankAccountsForUser(string userId);
    BankAccount? GetBankAccountForUser(string userId, int bankAccountId);
    void CreateBudgetaryPoolAccount(string userId, string name, PoolType poolType, BankAccount? bankAccount);
}