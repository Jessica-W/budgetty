using System.Diagnostics.CodeAnalysis;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;
using Budgetty.TestHelpers;
using Moq;

namespace Budgetty.Services.Tests
{
    [TestFixture]
    public class FinancialStateServiceTests : TestBase<FinancialStateService>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public async Task GivenUserId_WhenGetCurrentFinancialStateForUserAsyncIsCalled_ThenSnapshotAndEventsAndPoolsAreRetrievedAndUsedWithEventProcessorToGetFinancialState()
        {
            // Given
            var userId = "1234";
            var endDate = new DateOnly(2022, 12, 1);
            var budgetaryEvents = new List<BudgetaryEvent>();
            var pools = new List<BudgetaryPool>();
            var expectedFinancialState = new FinancialState(new List<BudgetaryPool>(), new List<BankAccount>(), null);

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(endDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(userId, null, endDate))
                .Returns(budgetaryEvents)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(userId, true, false))
                .Returns(pools)
                .Verifiable();

            GetMock<IEventProcessor>()
                .Setup(x => x.ProcessEvents(budgetaryEvents, null, pools, It.IsAny<Action<BudgetaryEvent, FinancialState>>()))
                .Returns(expectedFinancialState)
                .Verifiable();

            // When
            var result = await ClassUnderTest.GetCurrentFinancialStateForUserAsync(userId);

            // Then
            Assert.That(result, Is.EqualTo(expectedFinancialState));

            GetMock<IDateTimeProvider>().Verify();
            GetMock<IFinancialsSnapshotManager>().Verify(x => x.GetSnapshotAsync(userId), Times.Once);
            GetMock<IBudgetaryRepository>().Verify();
            GetMock<IEventProcessor>().Verify();
        }
    }
}