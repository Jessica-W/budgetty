using System.Security.Claims;
using AutoFixture;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Mvc.Controllers;
using Budgetty.Mvc.Models.Transactions;
using Budgetty.Persistance.Repositories;
using Budgetty.Services;
using Budgetty.Services.Interfaces;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Budgetty.Mvc.Tests.Controllers
{
    public class TestTransactionsController : TestBase<TransactionsController>
    {
        private const string UserId = "1234";

        protected override void SetUp()
        {
            GetMock<UserManager<IdentityUser>>()
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(UserId);
        }

        #region Index
        
        [Test]
        public void GivenBudgetaryEventsExist_WhenIndexIsCalled_ThenViewResultWithCorrectViewModelValuesIsReturned()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var monthEnd = new DateOnly(2022, 12, 31);

            var budgetaryEventA = new BudgetaryEvent
            {
                Id = 1,
                UserId = UserId,
                Date = now.AddDays(-1),
                Notes = "Notes",
            };

            var budgetaryEventB = new BudgetaryEvent
            {
                Id = 2,
                UserId = UserId,
                Date = now.AddDays(1),
                Notes = "More notes",
            };

            const string budgetaryEventADescription = "Description of budgetaryEventA";
            const string budgetaryEventBDescription = "Description of budgetaryEventB";

            var budgetaryEvents = new List<BudgetaryEvent>
            {
                budgetaryEventA,
                budgetaryEventB,
            };

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns(budgetaryEventA.Date)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns(budgetaryEventB.Date)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, monthStart, monthEnd))
                .Returns(budgetaryEvents)
                .Verifiable();

            GetMock<IBudgetaryEventDescriber>()
                .Setup(x => x.DescribeEvent(budgetaryEventA))
                .Returns(budgetaryEventADescription);

            GetMock<IBudgetaryEventDescriber>()
                .Setup(x => x.DescribeEvent(budgetaryEventB))
                .Returns(budgetaryEventBDescription);

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(budgetaryEventA.Date));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(budgetaryEventB.Date));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(monthStart));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(monthEnd));

                Assert.That(viewModel.Transactions, Has.Count.EqualTo(2));
                Assert.That(viewModel.Transactions[0].Date, Is.EqualTo(budgetaryEventA.Date));
                Assert.That(viewModel.Transactions[0].Notes, Is.EqualTo(budgetaryEventA.Notes));
                Assert.That(viewModel.Transactions[0].Description, Is.EqualTo(budgetaryEventADescription));

                Assert.That(viewModel.Transactions[1].Date, Is.EqualTo(budgetaryEventB.Date));
                Assert.That(viewModel.Transactions[1].Notes, Is.EqualTo(budgetaryEventB.Notes));
                Assert.That(viewModel.Transactions[1].Description, Is.EqualTo(budgetaryEventBDescription));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenTransactionsStartAndEndDateAreNotProvidedAndEventsExist_WhenIndexIsCalled_ThenBudgetaryEventsForTheCurrentMonthAreFetchedAndStartAndEndDatesAreMonthStartAndMonthEndRespectively()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var monthEnd = new DateOnly(2022, 12, 31);
            var earliestEventDate = monthStart.AddDays(-1);
            var latestEventDate = monthStart.AddDays(1);
            var budgetaryEvents = BuildObject<BudgetaryEvent>()
                .Without(x => x.SourcePool)
                .Without(x => x.DestinationPool)
                .CreateMany(2)
                .ToList();

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns(earliestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns(latestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, monthStart, monthEnd))
                .Returns(budgetaryEvents)
                .Verifiable();
            
            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(earliestEventDate));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(latestEventDate));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(monthStart));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(monthEnd));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenTransactionsStartAndEndDateAreNotProvidedAndEventsDoNotExist_WhenIndexIsCalled_ThenBudgetaryEventsForTheCurrentMonthAreFetchedAndStartAndEndDatesAreDateMinAndDateMaxRespectively()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var monthEnd = new DateOnly(2022, 12, 31);

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns((DateOnly?)null)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns((DateOnly?)null)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, monthStart, monthEnd))
                .Returns(new List<BudgetaryEvent>())
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(DateOnly.MinValue));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(DateOnly.MaxValue));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(monthStart));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(monthEnd));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenTransactionsStartAndEndDateAreNotProvidedAndEventsExistButAfterTheCurrentMonth_WhenIndexIsCalled_ThenBudgetaryEventsForTheFirstEventsMonthStartAreFetchedAndStartAndEndDatesAreMonthStartAndMonthEndOfTheFirstEventRespectively()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var earliestEventDate = monthStart.AddMonths(1);
            var latestEventDate = earliestEventDate.AddDays(1);
            var expectedTransactionsStartDate = earliestEventDate.MonthStart();
            var expectedTransactionsEndDate = latestEventDate.MonthEnd();

            var budgetaryEvents = BuildObject<BudgetaryEvent>()
                .Without(x => x.SourcePool)
                .Without(x => x.DestinationPool)
                .CreateMany(2)
                .ToList();

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns(earliestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns(latestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, expectedTransactionsStartDate, expectedTransactionsEndDate))
                .Returns(budgetaryEvents)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(earliestEventDate));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(latestEventDate));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(expectedTransactionsStartDate));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(expectedTransactionsEndDate));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenTransactionsStartAndEndDateAreProvidedAndDatesAreNotMoreThanNinetyDaysApart_WhenIndexIsCalled_ThenBudgetaryEventsForTheRequestedStartAndEndDatesAreFetchedAndStartAndEndDateInViewModelMatchRequestedDates()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var earliestEventDate = monthStart.AddMonths(1);
            var latestEventDate = earliestEventDate.AddDays(1);
            var requestedStartDate = now;
            var requestedEndDate = now.AddDays(90);

            var budgetaryEvents = BuildObject<BudgetaryEvent>()
                .Without(x => x.SourcePool)
                .Without(x => x.DestinationPool)
                .CreateMany(2)
                .ToList();

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns(earliestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns(latestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, requestedStartDate, requestedEndDate))
                .Returns(budgetaryEvents)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index(requestedStartDate, requestedEndDate) as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(earliestEventDate));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(latestEventDate));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(requestedStartDate));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(requestedEndDate));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenTransactionsStartAndEndDateAreProvidedAndDatesAreMoreThanNinetyDaysApartAndEventsExistForTheCurrentMonth_WhenIndexIsCalled_ThenStartAndEndDateAreSetToNullAndCurrentMonthIsUsed()
        {
            // Given
            var now = new DateOnly(2022, 12, 17);
            var monthStart = new DateOnly(2022, 12, 1);
            var monthEnd = new DateOnly(2022, 12, 31);
            var earliestEventDate = monthStart.AddDays(1);
            var latestEventDate = earliestEventDate.AddDays(1);
            var requestedStartDate = now;
            var requestedEndDate = now.AddDays(91);

            var budgetaryEvents = BuildObject<BudgetaryEvent>()
                .Without(x => x.SourcePool)
                .Without(x => x.DestinationPool)
                .CreateMany(2)
                .ToList();

            GetMock<IDateTimeProvider>()
                .Setup(x => x.GetDateNow())
                .Returns(now);

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfEarliestBudgetaryEventForUser(UserId))
                .Returns(earliestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetDateOfLatestBudgetaryEventForUser(UserId))
                .Returns(latestEventDate)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryEventsForUser(UserId, monthStart, monthEnd))
                .Returns(budgetaryEvents)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index(requestedStartDate, requestedEndDate) as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as TransactionsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.EarliestTransaction, Is.EqualTo(earliestEventDate));
                Assert.That(viewModel.LatestTransaction, Is.EqualTo(latestEventDate));
                Assert.That(viewModel.TransactionsStartDate, Is.EqualTo(monthStart));
                Assert.That(viewModel.TransactionsEndDate, Is.EqualTo(monthEnd));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        #endregion
    }
}