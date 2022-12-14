using Budgetty.Domain;
using Budgetty.Persistance;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Budgetty.Services
{
    public class UserInitializer : IUserInitializer
    {
        private readonly ISequenceNumberProvider _sequenceNumberProvider;
        private readonly ISnapshotLockManager _snapshotLockManager;
        private readonly IBudgetaryRepository _budgetaryRepository;
        private readonly IBudgetaryEventFactory _budgetaryEventFactory;

        public UserInitializer(ISequenceNumberProvider sequenceNumberProvider, ISnapshotLockManager snapshotLockManager, IBudgetaryRepository budgetaryRepository, IBudgetaryEventFactory budgetaryEventFactory)
        {
            _sequenceNumberProvider = sequenceNumberProvider;
            _snapshotLockManager = snapshotLockManager;
            _budgetaryRepository = budgetaryRepository;
            _budgetaryEventFactory = budgetaryEventFactory;
        }

        public async Task InitializeUser(IdentityUser user)
        {
            await _sequenceNumberProvider.InitialiseSequenceNumberAsync(user.Id);
            await _snapshotLockManager.InitialiseLock(user.Id);

            CreateTestData(user.Id);
        }

        private void CreateTestData(string userId)
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow);

            var bankAccountA = new BankAccount
            {
                Name = "Current",
                UserId = userId,
            };

            var bankAccountB = new BankAccount
            {
                Name = "Savings",
                UserId = userId,
            };

            var incomePoolA = new BudgetaryPool
            {
                Type = PoolType.Income,
                Name = "Test Income Pool A",
                UserId = userId,
                BankAccount = bankAccountA,
            };

            var incomePoolB = new BudgetaryPool
            {
                Type = PoolType.Income,
                Name = "Test Income Pool B",
                UserId = userId,
                BankAccount = bankAccountA,
            };

            var incomePoolC = new BudgetaryPool
            {
                Type = PoolType.Income,
                Name = "Test Income Pool C",
                UserId = userId,
                BankAccount = bankAccountB,
            };

            var incomePoolD = new BudgetaryPool
            {
                Type = PoolType.Income,
                Name = "Test Income Pool D",
                UserId = userId,
                BankAccount = bankAccountB,
            };

            var debtPoolA = new BudgetaryPool
            {
                Type = PoolType.Debt,
                Name = "Debt Pool A",
                UserId = userId,
            };

            _budgetaryRepository.AddBudgetaryPool(incomePoolA);
            _budgetaryRepository.AddBudgetaryPool(incomePoolB);
            _budgetaryRepository.AddBudgetaryPool(incomePoolC);
            _budgetaryRepository.AddBudgetaryPool(incomePoolD);
            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeEvent(now, userId, 70));
            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeEvent(now, userId, 100, debtPoolA));

            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 25, incomePoolA));
            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 20, incomePoolB));
            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 10, incomePoolC));
            _budgetaryRepository.AddBudgetaryEvent(_budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 5, incomePoolD));

            _budgetaryRepository.SaveChanges();
        }
    }
}