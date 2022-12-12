using Budgetty.Domain;
using Budgetty.Persistance;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Budgetty.Mvc.Identity;

public class ApplicationUserManager : UserManager<IdentityUser>
{
    private readonly ISequenceNumberProvider _sequenceNumberProvider;
    private readonly ISnapshotLockManager _snapshotLockManager;
    private readonly IBudgetaryRepository _budgetaryRepository;
    private readonly IBudgetaryEventFactory _budgetaryEventFactory;
    
    public ApplicationUserManager(IUserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators, IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger, ISequenceNumberProvider sequenceNumberProvider, ISnapshotLockManager snapshotLockManager, IBudgetaryRepository budgetaryRepository, IBudgetaryEventFactory budgetaryEventFactory) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _sequenceNumberProvider = sequenceNumberProvider;
        _snapshotLockManager = snapshotLockManager;
        _budgetaryRepository = budgetaryRepository;
        _budgetaryEventFactory = budgetaryEventFactory;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user)
    {
        var result = await base.CreateAsync(user);

        if (result.Succeeded)
        {
            await _sequenceNumberProvider.InitialiseSequenceNumberAsync(user.Id);
            await _snapshotLockManager.InitialiseLock(user.Id);

            CreateTestData(user.Id);
        }

        return result;
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