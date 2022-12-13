using AutoFixture;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.TestHelpers;

namespace Budgetty.Services.Tests
{
    [TestFixture]
    public class TestEventProcessor : TestBase<EventProcessor>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public void GivenInvalidEventType_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = (BudgetaryEvent.BudgetaryEventType)(-50),
                    AmountInPennies = 42,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                },
            };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, null, new List<BudgetaryPool>()));
            Assert.That(ex!.Message, Is.EqualTo($"Unexpected event type ({allEvents[0].EventType}) (Parameter 'budgetaryEvent')"));
        }

        [Test]
        public void GivenBudgetaryEventsAndAFinancialSnapshotAndNoPoolsAndCallback_WhenProcessEventsIsCalled_ThenCallBackIsCalledForEachEvent()
        {
            // Given
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Income,
                    AmountInPennies = 42,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                },
            };
            FinancialsSnapshot? financialsSnapshot = null;
            var pools = new List<BudgetaryPool>();
            var callbackCounter = 0;

            // When
            ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools, (_, _) => { callbackCounter++; });

            // Then
            Assert.That(callbackCounter, Is.EqualTo(allEvents.Count));
        }

        [Test]
        public void GivenIncomeBudgetaryEventAndABlankFinancialSnapshotAndNoPools_WhenProcessEventsIsCalled_ThenUnallocatedIncomeMatchesIncomeAmount()
        {
            // Given
            const int incomeAmount = 42;

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Income,
                    AmountInPennies = incomeAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                },
            };
            FinancialsSnapshot? financialsSnapshot = null;
            var pools = new List<BudgetaryPool>();
            
            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UnallocatedIncomeInPennies, Is.EqualTo(incomeAmount));
        }

        [Test]
        public void GivenIncomeBudgetaryEventWithDebtPoolAndABlankFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenUnallocatedIncomeMatchesIncomeAmountAsDoesDebtPoolBalance()
        {
            // Given
            const int incomeAmount = 42;

            var debtPool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Debt)
                .Without(x => x.BankAccount)
                .Create();

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Income,
                    AmountInPennies = incomeAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = debtPool,
                },
            };
            FinancialsSnapshot? financialsSnapshot = null;
            var pools = new List<BudgetaryPool> { debtPool };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.UnallocatedIncomeInPennies, Is.EqualTo(incomeAmount));
                Assert.That(result.GetPoolBalance(debtPool), Is.EqualTo(incomeAmount));
            });
        }

        [Test]
        public void GivenIncomeBudgetaryEventWithIncomePoolAndABlankFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int incomeAmount = 42;

            var incomePool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Income)
                .Without(x => x.BankAccount)
                .Create();

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Income,
                    AmountInPennies = incomeAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = incomePool,
                },
            };
            FinancialsSnapshot? financialsSnapshot = null;
            var pools = new List<BudgetaryPool> { incomePool };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));

            Assert.That(ex!.Message, Is.EqualTo("Source pool must be a debt pool (Parameter 'incomeEvent')"));
        }

        [Test]
        public void GivenExpenditureBudgetaryEventAndAFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenPoolAndBankAccountBalanceCorrectlyAdjusted()
        {
            // Given
            const int expenditureAmount = 42;
            const int poolBalance = 50;

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current",
            };

            var pool = new BudgetaryPool
            {
                Id = 1,
                UserId = "1234",
                Type = PoolType.Income,
                BankAccount = bankAccount,
                Name = "General",
            };

            bankAccount.BudgetaryPools.Add(pool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                    AmountInPennies = expenditureAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SourcePool = pool,
                    SequenceNumber = 1,
                },
            };
            var financialsSnapshot = new FinancialsSnapshot
            {
                UserId = "1234",
                Id = 1,
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = pool,
                        BalanceInPennies = poolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BalanceInPennies = poolBalance,
                        BankAccount = bankAccount,
                    },
                },
            };
            var pools = new List<BudgetaryPool> { pool };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);
            
            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPoolBalance(pool), Is.EqualTo(poolBalance - expenditureAmount));
                Assert.That(result.GetBankAccountBalance(bankAccount), Is.EqualTo(poolBalance - expenditureAmount));
            });
        }

        [Test]
        public void GivenExpenditureBudgetaryEventWithInvalidSourcePoolTypeAndAFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int expenditureAmount = 42;
            const int poolBalance = 50;
            
            var pool = new BudgetaryPool
            {
                Id = 1,
                UserId = "1234",
                Type = PoolType.Debt,
                Name = "General",
            };
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                    AmountInPennies = expenditureAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SourcePool = pool,
                    SequenceNumber = 1,
                },
            };
            var financialsSnapshot = new FinancialsSnapshot
            {
                UserId = "1234",
                Id = 1,
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = pool,
                        BalanceInPennies = poolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>(),
            };
            var pools = new List<BudgetaryPool>{ pool };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));

            Assert.That(ex!.Message, Is.EqualTo($"Unable to expend from \"{pool.Name}\". Expenditures from a debt pools are disallowed. (Parameter 'expenditureEvent')"));
        }

        [Test]
        public void GivenExpenditureBudgetaryEventWithNullSourcePoolAndABlankFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int expenditureAmount = 42;
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                    AmountInPennies = expenditureAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SourcePool = null,
                    SequenceNumber = 1,
                },
            };
            var financialsSnapshot = new FinancialsSnapshot
            {
                UserId = "1234",
                Id = 1,
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>(),
                BankAccountSnapShots = new List<BankAccountSnapShot>(),
            };
            var pools = new List<BudgetaryPool>();

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));

            Assert.That(ex!.Message, Is.EqualTo("Source pool cannot be null (Parameter 'expenditureEvent')"));
        }

        [Test]
        public void GivenExpenditureBudgetaryEventThatExpendsMoreThanIsAvailableAndAFinancialSnapshotAndPools_WhenProcessEventsIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            const int expenditureAmount = 42;
            const int poolBalance = 30;

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current",
            };

            var pool = new BudgetaryPool
            {
                Id = 1,
                UserId = "1234",
                Type = PoolType.Income,
                BankAccount = bankAccount,
                Name = "General",
            };

            bankAccount.BudgetaryPools.Add(pool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                    AmountInPennies = expenditureAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SourcePool = pool,
                    SequenceNumber = 1,
                },
            };
            var financialsSnapshot = new FinancialsSnapshot
            {
                UserId = "1234",
                Id = 1,
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = pool,
                        BalanceInPennies = poolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BalanceInPennies = poolBalance,
                        BankAccount = bankAccount,
                    },
                },
            };
            var pools = new List<BudgetaryPool> { pool };

            // When / Then
            var ex = Assert.Throws<InvalidOperationException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));

            Assert.That(ex!.Message, Is.EqualTo($"Expenditure of £{allEvents[0].AmountInPennies / 100m:0.00} greater than balance in pool \"{pool.Name}\""));
        }
    }
}