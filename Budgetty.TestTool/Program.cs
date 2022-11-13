using Autofac;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.Persistance.DependencyInjection;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Budgetty.TestTool;

public static class Program
{
    public static void Main()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<ServicesModule>();
        builder.RegisterModule<PersistenceModule>();
        RegisterDbContext(builder);

        var container = builder.Build();

        using (var scope = container.BeginLifetimeScope())
        {
            var dbContext = scope.Resolve<BudgettyDbContext>();
            var budgetaryRepository = scope.Resolve<IBudgetaryRepository>();
            var sequenceNumberProvider = scope.Resolve<ISequenceNumberProvider>();

            var userId = dbContext.Users.First().Id;
            CreateTestData(budgetaryRepository, sequenceNumberProvider, userId);
        }
    }

    private static void CreateTestData(IBudgetaryRepository budgetaryRepository, ISequenceNumberProvider sequenceNumberProvider, string userId)
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

        budgetaryRepository.AddBudgetaryPool(incomePoolA);
        budgetaryRepository.AddBudgetaryPool(incomePoolB);
        budgetaryRepository.AddBudgetaryPool(incomePoolC);
        budgetaryRepository.AddBudgetaryPool(incomePoolD);

        budgetaryRepository.AddBudgetaryEvent(new IncomeEvent
        {
            Date = now,
            UserId = userId,
            AmountInPennies = 70,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.AddBudgetaryEvent(new IncomeEvent
        {
            Date = now,
            UserId = userId,
            AmountInPennies = 100,
            DebtPool = debtPoolA,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 25,
            Date = now,
            Pool = incomePoolA,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 20,
            Date = now,
            Pool = incomePoolB,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 10,
            Date = now,
            Pool = incomePoolC,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.AddBudgetaryEvent(new IncomeAllocationEvent
        {
            UserId = userId,
            AmountInPennies = 5,
            Date = now,
            Pool = incomePoolD,
            SequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId),
        });

        budgetaryRepository.SaveChanges();
    }

    private static void AddEvent(ISequenceNumberProvider sequenceNumberProvider, string userId, BudgettyDbContext dbContext)
    {
        var sequenceNumber = sequenceNumberProvider.GetNextSequenceNumber(userId);
        dbContext.BudgetaryEvents.Add(new IncomeEvent
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow), AmountInPennies = 20, UserId = userId,
            SequenceNumber = sequenceNumber
        });
    }

    private static void RegisterDbContext(ContainerBuilder builder)
    {
        const string connectionString =
            "server=10.10.3.159;port=3307;database=Budgetty;user=budgetty;password=6SmilingSausagesAteTheBeans!";
        var optionsBuilder = new DbContextOptionsBuilder<BudgettyDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        var options = optionsBuilder.Options;

        builder.Register(_ =>
        {
            var dbContext = new BudgettyDbContext(options);
            return dbContext;
        });

        builder.Register(_ => new SimpleDbContextFactory(options)).As<IDbContextFactory<BudgettyDbContext>>();
    }

    private sealed class SimpleDbContextFactory : IDbContextFactory<BudgettyDbContext>
    {
        private readonly DbContextOptions<BudgettyDbContext> _dbContextOptions;

        public SimpleDbContextFactory(DbContextOptions<BudgettyDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public BudgettyDbContext CreateDbContext()
        {
            return new BudgettyDbContext(_dbContextOptions);
        }
    }
}