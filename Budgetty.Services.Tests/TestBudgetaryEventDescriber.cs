using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.TestHelpers;

namespace Budgetty.Services.Tests
{
    public class TestBudgetaryEventDescriber : TestBase<BudgetaryEventDescriber>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public void GivenIncomeEventWithDebtPool_WhenDescribeEventIsCalled_ThenCorrectDescriptionIsReturned()
        {
            // Given
            var sourcePool = new BudgetaryPool
            {
                Type = PoolType.Debt,
                Name = "Debt",
            };

            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Income,
                AmountInPennies = 150,
                SourcePool = sourcePool,
            };

            // When
            var result = ClassUnderTest.DescribeEvent(budgetaryEvent);

            // Then
            Assert.That(result, Is.EqualTo($"Borrowed £1.50 from Debt"));
        }

        [Test]
        public void GivenIncomeEventWithoutDebtPool_WhenDescribeEventIsCalled_ThenCorrectDescriptionIsReturned()
        {
            // Given
            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Income,
                AmountInPennies = 150,
            };

            // When
            var result = ClassUnderTest.DescribeEvent(budgetaryEvent);

            // Then
            Assert.That(result, Is.EqualTo($"Income of £1.50"));
        }

        [Test]
        public void GivenIncomeAllocationEvent_WhenDescribeEventIsCalled_ThenCorrectDescriptionIsReturned()
        {
            // Given
            var destinationPool = new BudgetaryPool
            {
                Name = "Current",
            };

            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.IncomeAllocation,
                AmountInPennies = 150,
                DestinationPool = destinationPool,
            };

            // When
            var result = ClassUnderTest.DescribeEvent(budgetaryEvent);

            // Then
            Assert.That(result, Is.EqualTo($"£1.50 of income allocated to Current"));
        }

        [Test]
        public void GivenExpenditureEvent_WhenDescribeEventIsCalled_ThenCorrectDescriptionIsReturned()
        {
            // Given
            var sourcePool = new BudgetaryPool
            {
                Name = "Current",
            };

            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.Expenditure,
                AmountInPennies = 50,
                SourcePool= sourcePool,
            };

            // When
            var result = ClassUnderTest.DescribeEvent(budgetaryEvent);

            // Then
            Assert.That(result, Is.EqualTo("Expenditure of £0.50 from Current"));
        }

        [Test]
        public void GivenPoolTransferEvent_WhenDescribeEventIsCalled_ThenCorrectDescriptionIsReturned()
        {
            // Given
            var sourcePool = new BudgetaryPool
            {
                Name = "Current",
            };
            var destinationPool = new BudgetaryPool
            {
                Name = "Savings",
            };

            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = BudgetaryEvent.BudgetaryEventType.PoolTransfer,
                AmountInPennies = 50,
                SourcePool = sourcePool,
                DestinationPool = destinationPool,
            };

            // When
            var result = ClassUnderTest.DescribeEvent(budgetaryEvent);

            // Then
            Assert.That(result, Is.EqualTo("Transfer of £0.50 from Current to Savings"));
        }

        [Test]
        public void GivenUnknownBudgetaryEventType_WhenDescribeEventIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            var budgetaryEvent = new BudgetaryEvent
            {
                EventType = (BudgetaryEvent.BudgetaryEventType)(-1),
            };

            // When / Then
            var ex = Assert.Throws<ArgumentException>(() => { ClassUnderTest.DescribeEvent(budgetaryEvent); });
            Assert.That(ex!.Message, Is.EqualTo("Unknown budget type (Parameter 'budgetaryEvent')"));
        }
    }
}