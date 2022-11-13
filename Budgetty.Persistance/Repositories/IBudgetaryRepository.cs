﻿using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;

namespace Budgetty.Persistance.Repositories;

public interface IBudgetaryRepository
{
    DateOnly? GetDateOfEarliestBudgetaryEventForUser(string userId);
    DateOnly? GetDateOfLatestBudgetaryEventForUser(string userId);
    IEnumerable<BudgetaryEvent> GetBudgetaryEventsForUser(string userId, DateOnly? startDate = null, DateOnly? endDate = null);
    IEnumerable<BudgetaryPool> GetBudgetaryPoolsForUser(string userId, bool includeBankAccounts);
    void AddBudgetaryPool(BudgetaryPool pool);
    void AddBudgetaryEvent(BudgetaryEvent budgetaryEvent);
    void SaveChanges();
}