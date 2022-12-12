using Autofac;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.Persistance.DependencyInjection;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.DependencyInjection;
using Budgetty.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var budgetaryEventFactory = scope.Resolve<IBudgetaryEventFactory>();

            var userId = dbContext.Users.First().Id;
            CreateTestData(budgetaryRepository, budgetaryEventFactory, userId);
        }
    }

    private static void CreateTestData(IBudgetaryRepository budgetaryRepository, IBudgetaryEventFactory budgetaryEventFactory, string userId)
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

        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeEvent(now, userId, 70));
        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeEvent(now, userId, 100, debtPoolA));
        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 25, incomePoolA));
        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 20, incomePoolB));
        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 10, incomePoolC));
        budgetaryRepository.AddBudgetaryEvent(budgetaryEventFactory.CreateIncomeAllocationEvent(now, userId, 5, incomePoolD));

        budgetaryRepository.SaveChanges();
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