using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;
using Moq;

namespace Budgetty.Services.Tests
{
    public class FinancialStateServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GivenUserId_WhenGetCurrentFinancialStateForUserAsyncIsCalled_ThenSnapshotAndEventsAndPoolsAreRetrievedAndUsedWithEventProcessorToGetFinancialState()
        {
            // Given
            var mockFinancialSnapshotManager = new Mock<IFinancialsSnapshotManager>();
            var mockBudgetaryRepository = new Mock<IBudgetaryRepository>();
            var mockEventProcessor = new Mock<IEventProcessor>();
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            var userId = "1234";
            var endDate = new DateOnly(2022, 12, 1);
            var budgetaryEvents = new List<BudgetaryEvent>();
            var pools = new List<BudgetaryPool>();
            var expectedFinancialState = new FinancialState(new List<BudgetaryPool>(), new List<BankAccount>(), null);

            mockDateTimeProvider
                .Setup(x => x.GetDateNow())
                .Returns(endDate)
                .Verifiable();

            mockBudgetaryRepository
                .Setup(x => x.GetBudgetaryEventsForUser(userId, null, endDate))
                .Returns(budgetaryEvents)
                .Verifiable();

            mockBudgetaryRepository
                .Setup(x => x.GetBudgetaryPoolsForUser(userId, true, false))
                .Returns(pools)
                .Verifiable();

            mockEventProcessor
                .Setup(x => x.ProcessEvents(budgetaryEvents, null, pools, It.IsAny<Action<BudgetaryEvent, FinancialState>>()))
                .Returns(expectedFinancialState)
                .Verifiable();

            var uut = new FinancialStateService(mockFinancialSnapshotManager.Object, mockBudgetaryRepository.Object, mockEventProcessor.Object, mockDateTimeProvider.Object);

            // When
            var result = await uut.GetCurrentFinancialStateForUserAsync(userId);

            // Then
            Assert.That(result, Is.EqualTo(expectedFinancialState));

            mockDateTimeProvider.Verify();
            mockFinancialSnapshotManager.Verify(x => x.GetSnapshotAsync(userId), Times.Once);
            mockBudgetaryRepository.Verify();
            mockEventProcessor.Verify();
        }
    }
}