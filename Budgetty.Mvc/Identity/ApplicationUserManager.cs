using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.Persistance.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Budgetty.Mvc.Identity;

public class ApplicationUserManager : UserManager<IdentityUser>
{
    private readonly ISequenceNumberProvider _sequenceNumberProvider;
    private readonly ISnapshotLockManager _snapshotLockManager;
    private readonly IBudgetaryRepository _budgetaryRepository;
    
    public ApplicationUserManager(IUserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators, IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger, ISequenceNumberProvider sequenceNumberProvider, ISnapshotLockManager snapshotLockManager, IBudgetaryRepository budgetaryRepository) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _sequenceNumberProvider = sequenceNumberProvider;
        _snapshotLockManager = snapshotLockManager;
        _budgetaryRepository = budgetaryRepository;
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

        _budgetaryRepository.AddBudgetaryEvent(new IncomeEvent
        {
            Date = now,
            UserId = userId,
            AmountInPennies = 70,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.AddBudgetaryEvent(new IncomeEvent
        {
            Date = now,
            UserId = userId,
            AmountInPennies = 100,
            DebtPool = debtPoolA,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 25,
            Date = now,
            Pool = incomePoolA,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 20,
            Date = now,
            Pool = incomePoolB,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 10,
            Date = now,
            Pool = incomePoolC,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 5,
            Date = now,
            Pool = incomePoolD,
            SequenceNumber = _sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        _budgetaryRepository.SaveChanges();
    }
}