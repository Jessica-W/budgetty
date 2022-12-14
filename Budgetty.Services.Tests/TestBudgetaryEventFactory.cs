using AutoFixture;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance;
using Budgetty.TestHelpers;

namespace Budgetty.Services.Tests
{
    [TestFixture]
    public class TestBudgetaryEventFactory : TestBase<BudgetaryEventFactory>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public void GivenIncomeEventDetails_WhenCreateIncomeEventIsCalled_ThenBudgetaryEventWithCorrectTypeAndSequenceNumberAndProvidedDetailsIsReturned()
        {
            // Given
            var date = new DateOnly(2022, 12, 13);
            const string userId = "1234";
            const int amountInPennies = 42;
            var debtPool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Debt)
                .Without(x => x.BankAccount)
                .Create();
            const int expectedSequenceNumber = 2;

            GetMock<ISequenceNumberProvider>()
                .Setup(x => x.GetNextSequenceNumber(userId))
                .Returns(expectedSequenceNumber)
                .Verifiable();

            // When
            var result = ClassUnderTest.CreateIncomeEvent(date, userId, amountInPennies, debtPool);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.EventType, Is.EqualTo(BudgetaryEvent.BudgetaryEventType.Income));
                Assert.That(result.Date, Is.EqualTo(date));
                Assert.That(result.UserId, Is.EqualTo(userId));
                Assert.That(result.AmountInPennies, Is.EqualTo(amountInPennies));
                Assert.That(result.SourcePool, Is.EqualTo(debtPool));
                Assert.That(result.DestinationPool, Is.Null);
                Assert.That(result.SequenceNumber, Is.EqualTo(expectedSequenceNumber));
            });

            GetMock<ISequenceNumberProvider>().Verify();
        }

        [Test]
        public void GivenIncomeEventDetails_WhenCreateIncomeAllocationEventIsCalled_ThenBudgetaryEventWithCorrectTypeAndSequenceNumberAndProvidedDetailsIsReturned()
        {
            // Given
            var date = new DateOnly(2022, 12, 13);
            const string userId = "1234";
            const int amountInPennies = 42;
            var pool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Income)
                .Create();
            const int expectedSequenceNumber = 2;

            GetMock<ISequenceNumberProvider>()
                .Setup(x => x.GetNextSequenceNumber(userId))
                .Returns(expectedSequenceNumber)
                .Verifiable();

            // When
            var result = ClassUnderTest.CreateIncomeAllocationEvent(date, userId, amountInPennies, pool);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.EventType, Is.EqualTo(BudgetaryEvent.BudgetaryEventType.IncomeAllocation));
                Assert.That(result.Date, Is.EqualTo(date));
                Assert.That(result.UserId, Is.EqualTo(userId));
                Assert.That(result.AmountInPennies, Is.EqualTo(amountInPennies));
                Assert.That(result.SourcePool, Is.Null);
                Assert.That(result.DestinationPool, Is.EqualTo(pool));
                Assert.That(result.SequenceNumber, Is.EqualTo(expectedSequenceNumber));
            });

            GetMock<ISequenceNumberProvider>().Verify();
        }

        [Test]
        public void GivenIncomeEventDetails_WhenCreateExpenditureEventIsCalled_ThenBudgetaryEventWithCorrectTypeAndSequenceNumberAndProvidedDetailsIsReturned()
        {
            // Given
            var date = new DateOnly(2022, 12, 13);
            const string userId = "1234";
            const int amountInPennies = 42;
            var pool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Income)
                .Create();
            const int expectedSequenceNumber = 2;

            GetMock<ISequenceNumberProvider>()
                .Setup(x => x.GetNextSequenceNumber(userId))
                .Returns(expectedSequenceNumber)
                .Verifiable();

            // When
            var result = ClassUnderTest.CreateExpenditureEvent(date, userId, amountInPennies, pool);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.EventType, Is.EqualTo(BudgetaryEvent.BudgetaryEventType.Expenditure));
                Assert.That(result.Date, Is.EqualTo(date));
                Assert.That(result.UserId, Is.EqualTo(userId));
                Assert.That(result.AmountInPennies, Is.EqualTo(amountInPennies));
                Assert.That(result.SourcePool, Is.EqualTo(pool));
                Assert.That(result.DestinationPool, Is.Null);
                Assert.That(result.SequenceNumber, Is.EqualTo(expectedSequenceNumber));
            });

            GetMock<ISequenceNumberProvider>().Verify();
        }

        [Test]
        public void GivenIncomeEventDetails_WhenCreatePoolTransferEventIsCalled_ThenBudgetaryEventWithCorrectTypeAndSequenceNumberAndProvidedDetailsIsReturned()
        {
            // Given
            var date = new DateOnly(2022, 12, 13);
            const string userId = "1234";
            const int amountInPennies = 42;
            var sourcePool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Income)
                .Create();
            var destinationPool = BuildObject<BudgetaryPool>()
                .With(x => x.Type, PoolType.Debt)
                .Without(x => x.BankAccount)
                .Create();

            const int expectedSequenceNumber = 2;

            GetMock<ISequenceNumberProvider>()
                .Setup(x => x.GetNextSequenceNumber(userId))
                .Returns(expectedSequenceNumber)
                .Verifiable();

            // When
            var result = ClassUnderTest.CreatePoolTransferEvent(date, userId, amountInPennies, sourcePool, destinationPool);

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.EventType, Is.EqualTo(BudgetaryEvent.BudgetaryEventType.Expenditure));
                Assert.That(result.Date, Is.EqualTo(date));
                Assert.That(result.UserId, Is.EqualTo(userId));
                Assert.That(result.AmountInPennies, Is.EqualTo(amountInPennies));
                Assert.That(result.SourcePool, Is.EqualTo(sourcePool));
                Assert.That(result.DestinationPool, Is.EqualTo(destinationPool));
                Assert.That(result.SequenceNumber, Is.EqualTo(expectedSequenceNumber));
            });

            GetMock<ISequenceNumberProvider>().Verify();
        }
    }
}