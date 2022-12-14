using Budgetty.Domain;
using Budgetty.Mvc.Mappers;
using Budgetty.Services.Interfaces;
using Budgetty.TestHelpers;

namespace Budgetty.Mvc.Tests.Mappers
{
    public class TestFinancialStateMapper : TestBase<FinancialStateMapper>
    {
        private readonly DateTime _now = new DateTime(2022, 12, 14, 1, 31, 0, DateTimeKind.Utc);

        protected override void SetUp()
        {
            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetUtcNow())
                .Returns(_now);
        }

        [Test]
        public void GivenFinancialState_WhenMapToSummaryViewModelIsCalled_ThenCorrectViewModelIsReturned()
        {
            // Given
            const int incomePoolBalance = 42;
            const int debtPoolBalance = 37;

            var currentAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var incomePool = new BudgetaryPool
            {
                Id = 1,
                UserId = "1234",
                Type = PoolType.Income,
                Name = "Current",
                BankAccount = currentAccount,
            };

            currentAccount.BudgetaryPools.Add(incomePool);

            var debtPool = new BudgetaryPool
            {
                Id = 2,
                UserId = "1234",
                Type = PoolType.Debt,
                Name = "Debt",
            };

            var pools = new List<BudgetaryPool> { incomePool, debtPool };
            var bankAccounts = new List<BankAccount> { currentAccount };
            var financialSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 13),
                UnallocatedIncomeInPennies = 42,
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = incomePool,
                        BalanceInPennies = incomePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = debtPool,
                        BalanceInPennies = debtPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = currentAccount,
                        BalanceInPennies = incomePoolBalance,
                    },
                },
            };

            var financialState = new FinancialState(pools, bankAccounts, financialSnapshot);

            // When
            var result = ClassUnderTest.MapToSummaryViewModel(financialState);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.UnallocatedIncome, Is.EqualTo(financialState.UnallocatedIncomeInPennies / 100m));

                Assert.That(result.CurrentDate.Year, Is.EqualTo(_now.Year));
                Assert.That(result.CurrentDate.Month, Is.EqualTo(_now.Month));
                Assert.That(result.CurrentDate.Day, Is.EqualTo(_now.Day));

                Assert.That(result.DebtPoolBalances, Has.Count.EqualTo(1));
                Assert.That(result.DebtPoolBalances[0].PoolName, Is.EqualTo(debtPool.Name));
                Assert.That(result.DebtPoolBalances[0].Balance, Is.EqualTo(-debtPoolBalance / 100m));

                Assert.That(result.BankAccounts, Has.Count.EqualTo(1));
                Assert.That(result.BankAccounts[0].AccountBalance, Is.EqualTo(incomePoolBalance / 100m));
                Assert.That(result.BankAccounts[0].AccountName, Is.EqualTo(currentAccount.Name));
                Assert.That(result.BankAccounts[0].IncomePoolBalances, Has.Count.EqualTo(1));
                Assert.That(result.BankAccounts[0].IncomePoolBalances[0].PoolName, Is.EqualTo(incomePool.Name));
                Assert.That(result.BankAccounts[0].IncomePoolBalances[0].Balance, Is.EqualTo(incomePoolBalance / 100m));
            });
        }
    }
}