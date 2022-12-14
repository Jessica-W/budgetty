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

        #region Income Event

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

        #endregion

        #region Expenditure Event

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
            var pools = new List<BudgetaryPool> { pool };

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

        #endregion

        #region Income Allocation Event

        [Test]
        public void GivenIncomeAllocationEventDoesNotExceedAvailableFundsAndDestinationPoolIsIncomePool_WhenProcessEventsIsCalled_ThenUnallocatedIncomeIsReducedByTheAllocationAmmountAndTheIncomePoolAndAssociatedBankAccountBalanceIsIncreasedByTheSameAmount()
        {
            // Given
            const int incomeAmount = 42;
            const int allocationAmount = 30;

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Test Account",
            };

            var pool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccount,
                Name = "Savings",
            };

            bankAccount.BudgetaryPools.Add(pool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                    AmountInPennies = allocationAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    DestinationPool = pool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                UnallocatedIncomeInPennies = incomeAmount,
            };

            var pools = new List<BudgetaryPool> { pool };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.UnallocatedIncomeInPennies, Is.EqualTo(incomeAmount - allocationAmount));
                Assert.That(result.GetPoolBalance(pool), Is.EqualTo(allocationAmount));
                Assert.That(result.GetBankAccountBalance(bankAccount), Is.EqualTo(allocationAmount));
            });
        }

        [Test]
        public void GivenIncomeAllocationEventDoesNotExceedAvailableFundsAndDestinationPoolIsDebtPool_WhenProcessEventsIsCalled_ThenUnallocatedIncomeIsReducedByTheAllocationAmmountAndTheDebtPoolBalanceIsDecreasedByTheSameAmount()
        {
            // Given
            const int incomeAmount = 42;
            const int allocationAmount = 30;
            const int initialDebtAmount = 50;

            var pool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Debt,
                UserId = "1234",
                Name = "Debt",
            };

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                    AmountInPennies = allocationAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    DestinationPool = pool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                UnallocatedIncomeInPennies = incomeAmount,
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = pool,
                        BalanceInPennies = initialDebtAmount,
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
                Assert.That(result.UnallocatedIncomeInPennies, Is.EqualTo(incomeAmount - allocationAmount));
                Assert.That(result.GetPoolBalance(pool), Is.EqualTo(initialDebtAmount - allocationAmount));
            });
        }

        [Test]
        public void
            GivenIncomeAllocationEventDoesNotExceedAvailableFundsAndDestinationPoolIsDebtPoolAndAllocationAmountExceedsDebtAmount_WhenProcessEventsIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            const int incomeAmount = 42;
            const int allocationAmount = 30;
            const int initialDebtAmount = 20;

            var pool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Debt,
                UserId = "1234",
                Name = "Debt",
            };

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                    AmountInPennies = allocationAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    DestinationPool = pool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                UnallocatedIncomeInPennies = incomeAmount,
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = pool,
                        BalanceInPennies = initialDebtAmount,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { pool };

            // When / Then
            var ex = Assert.Throws<InvalidOperationException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Income allocation is greater than debt amount"));
        }

        [Test]
        public void GivenIncomeAllocationExceedsAvailableFunds_WhenProcessEventsIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            const int incomeAmount = 42;
            const int allocationAmount = 50;

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Test Account",
            };

            var pool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccount,
                Name = "Savings",
            };

            bankAccount.BudgetaryPools.Add(pool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                    AmountInPennies = allocationAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    DestinationPool = pool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                UnallocatedIncomeInPennies = incomeAmount,
            };

            var pools = new List<BudgetaryPool> { pool };

            // When / Then
            var ex = Assert.Throws<InvalidOperationException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Income allocation greater than unallocated income"));
        }

        [Test]
        public void GivenIncomeAllocationEventHasNullDestinationPool_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int incomeAmount = 42;
            const int allocationAmount = 30;
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                    AmountInPennies = allocationAmount,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    DestinationPool = null,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                UnallocatedIncomeInPennies = incomeAmount,
            };

            var pools = new List<BudgetaryPool>();

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Income allocation event has no destination pool (Parameter 'incomeAllocationEvent')"));
        }

        #endregion

        #region Pool Transfer Event

        [Test]
        public void
            GivenPoolTransferFromIncomePoolToIncomePoolThatDoesNotExceedAvailableFunds_WhenProcessEventsIsCalled_ThenBalanceInSourcePoolIsDecreasedByTheTransferAmountAndTheBalanceInTheDestinationPoolIsIncreasedByTheSameAmountConnectedBankAccountsAreUpdatedAccordingly()
        {
            // Given
            const int poolABalance = 42;
            const int poolBBalance = 30;
            const int transferAmonut = 12;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Test Account",
            };

            var poolA = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Savings",
            };

            bankAccountA.BudgetaryPools.Add(poolA);

            var bankAccountB = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Test Account",
            };

            var poolB = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountB,
                Name = "Savings",
            };

            bankAccountB.BudgetaryPools.Add(poolB);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = poolA,
                    DestinationPool = poolB,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = poolA,
                        BalanceInPennies = poolABalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = poolB,
                        BalanceInPennies = poolBBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = poolABalance,
                    },
                    new()
                    {
                        Id = 2,
                        BankAccount = bankAccountB,
                        BalanceInPennies = poolBBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { poolA, poolB };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPoolBalance(poolA), Is.EqualTo(poolABalance - transferAmonut));
                Assert.That(result.GetPoolBalance(poolB), Is.EqualTo(poolBBalance + transferAmonut));
                Assert.That(result.GetBankAccountBalance(bankAccountA), Is.EqualTo(poolABalance - transferAmonut));
                Assert.That(result.GetBankAccountBalance(bankAccountB), Is.EqualTo(poolBBalance + transferAmonut));
            });
        }

        [Test]
        public void
            GivenPoolTransferFromIncomePoolToIncomePoolThatExceedsAvailableFunds_WhenProcessEventsIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 70;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var sourcePool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(sourcePool);

            var bankAccountB = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Savings Account",
            };

            var destinationPool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountB,
                Name = "Savings",
            };

            bankAccountB.BudgetaryPools.Add(destinationPool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = sourcePool,
                    DestinationPool = destinationPool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = sourcePool,
                        BalanceInPennies = sourcePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = destinationPool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = sourcePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        BankAccount = bankAccountB,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { sourcePool, destinationPool };

            // When / Then
            var ex = Assert.Throws<InvalidOperationException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo($"Amount in source pool \"{sourcePool.Name}\" is less than the transfer amount of £{transferAmonut / 100m:0.00}"));
        }

        [Test]
        public void
            GivenPoolTransferWithNullSourcePool_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 70;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var destinationPool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(destinationPool);
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = null,
                    DestinationPool = destinationPool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 2,
                        Pool = destinationPool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = sourcePoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { destinationPool };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Source and destination pools must be provided (Parameter 'poolTransferEvent')"));
        }

        [Test]
        public void
            GivenPoolTransferWithNullDestinationPool_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 70;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var sourcePool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(sourcePool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = sourcePool,
                    DestinationPool = null,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 2,
                        Pool = sourcePool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = sourcePoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { sourcePool };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Source and destination pools must be provided (Parameter 'poolTransferEvent')"));
        }

        [Test]
        public void
            GivenPoolTransferFromIncomePoolToDebtPoolThatDoesNotExceedAvailableFundsOrDebtAmount_WhenProcessEventsIsCalled_ThenBalanceInSourcePoolIsDecreasedByTheTransferAmountAndTheBalanceInTheDestinationPoolIsAlsoDecreasedByTheSameAmountConnectedBankAccountIsUpdatedAccordingly()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 12;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var sourcePool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(sourcePool);
            
            var destinationPool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Debt,
                UserId = "1234",
                Name = "Debt",
            };
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = sourcePool,
                    DestinationPool = destinationPool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = sourcePool,
                        BalanceInPennies = sourcePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = destinationPool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = sourcePoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { sourcePool, destinationPool };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPoolBalance(sourcePool), Is.EqualTo(sourcePoolBalance - transferAmonut));
                Assert.That(result.GetPoolBalance(destinationPool), Is.EqualTo(destinationPoolBalance - transferAmonut));
                Assert.That(result.GetBankAccountBalance(bankAccountA), Is.EqualTo(sourcePoolBalance - transferAmonut));
            });
        }

        [Test]
        public void
            GivenPoolTransferFromIncomePoolToDebtPoolThatDoesNotExceedAvailableFundsButExceedsDebtAmount_WhenProcessEventsIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 32;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var sourcePool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(sourcePool);

            var destinationPool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Debt,
                UserId = "1234",
                Name = "Debt",
            };

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = sourcePool,
                    DestinationPool = destinationPool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = sourcePool,
                        BalanceInPennies = sourcePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = destinationPool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccountA,
                        BalanceInPennies = sourcePoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { sourcePool, destinationPool };

            // When / Then
            var ex = Assert.Throws<InvalidOperationException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Pool transfer amount is greater than debt amount"));
        }

        [Test]
        public void
            GivenPoolTransferFromPoolToItself_WhenProcessEventsIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const int poolBalance = 42;
            const int transferAmonut = 70;

            var bankAccountA = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Current Account",
            };

            var pool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccountA,
                Name = "Current",
            };

            bankAccountA.BudgetaryPools.Add(pool);
            
            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = pool,
                    DestinationPool = pool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
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
                        BankAccount = bankAccountA,
                        BalanceInPennies = poolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { pool };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools));
            Assert.That(ex!.Message, Is.EqualTo("Destination pool cannot be the same as the source pool (Parameter 'poolTransferEvent')"));
        }

        [Test]
        public void
            GivenPoolTransferFromIncomePoolToIncomePoolThatDoesNotExceedAvailableFundsAndThePoolsShareTheSameBankAccount_WhenProcessEventsIsCalled_ThenBalanceInSourcePoolIsDecreasedByTheTransferAmountAndTheBalanceInTheDestinationPoolIsIncreasedByTheSameAmountConnectedBankAccountBalanceRemainsTheSame()
        {
            // Given
            const int sourcePoolBalance = 42;
            const int destinationPoolBalance = 30;
            const int transferAmonut = 12;

            var bankAccount = new BankAccount
            {
                Id = 1,
                UserId = "1234",
                Name = "Test Account",
            };

            var sourcePool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccount,
                Name = "Current",
            };

            var destinationPool = new BudgetaryPool
            {
                Id = 1,
                Type = PoolType.Income,
                UserId = "1234",
                BankAccount = bankAccount,
                Name = "New Shoes",
            };

            bankAccount.BudgetaryPools.Add(sourcePool);
            bankAccount.BudgetaryPools.Add(destinationPool);

            var allEvents = new List<BudgetaryEvent>
            {
                new()
                {
                    EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                    AmountInPennies = transferAmonut,
                    Date = new DateOnly(2022, 12, 13),
                    SequenceNumber = 1,
                    SourcePool = sourcePool,
                    DestinationPool = destinationPool,
                },
            };

            var financialsSnapshot = new FinancialsSnapshot
            {
                Id = 1,
                UserId = "1234",
                Date = new DateOnly(2022, 12, 12),
                PoolSnapshots = new List<PoolSnapshot>
                {
                    new()
                    {
                        Id = 1,
                        Pool = sourcePool,
                        BalanceInPennies = sourcePoolBalance,
                    },
                    new()
                    {
                        Id = 2,
                        Pool = destinationPool,
                        BalanceInPennies = destinationPoolBalance,
                    },
                },
                BankAccountSnapShots = new List<BankAccountSnapShot>
                {
                    new()
                    {
                        Id = 1,
                        BankAccount = bankAccount,
                        BalanceInPennies = sourcePoolBalance + destinationPoolBalance,
                    },
                },
            };

            var pools = new List<BudgetaryPool> { sourcePool, destinationPool };

            // When
            var result = ClassUnderTest.ProcessEvents(allEvents, financialsSnapshot, pools);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.GetPoolBalance(sourcePool), Is.EqualTo(sourcePoolBalance - transferAmonut));
                Assert.That(result.GetPoolBalance(destinationPool), Is.EqualTo(destinationPoolBalance + transferAmonut));
                Assert.That(result.GetBankAccountBalance(bankAccount), Is.EqualTo(sourcePoolBalance + destinationPoolBalance));
            });
        }

        #endregion
    }
}