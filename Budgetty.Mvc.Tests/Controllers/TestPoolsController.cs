using System.Security.Claims;
using AutoFixture;
using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Budgetty.Mvc.Controllers;
using Budgetty.Mvc.Models.Pools;
using Budgetty.Persistance.Repositories;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Budgetty.Mvc.Tests.Controllers
{
    [TestFixture]
    public sealed class TestPoolsController : TestBase<PoolsController>
    {
        private const string UserId = "1234";

        protected override void SetUp()
        {
            GetMock<UserManager<IdentityUser>>()
                .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(UserId);
        }
        
        [Test]
        public void GivenPoolIdAndCurrentUser_WhenDeleteIsCalled_ThenBudgetaryRepositoryIsUsedToDeleteThePoolAndRedirectToIndexIsReturned()
        {
            // Given
            const int poolId = 42;
            bool deletePoolCalled = false;

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.DeletePool(UserId, poolId))
                .Callback(() => deletePoolCalled = true)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.SaveChanges())
                .Callback(() => {
                    if (!deletePoolCalled)
                    {
                        Assert.Fail("SaveChanges called on repository before pool was deleted");
                    }
                })
                .Verifiable();

            // When
            var result = ClassUnderTest.Delete(poolId) as RedirectToActionResult;

            // Then
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ControllerName, Is.Null);
            Assert.That(result.ActionName, Is.EqualTo(nameof(PoolsController.Index)));

            GetMock<IBudgetaryRepository>().Verify();
            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenCurrentUserHasOnePoolWithConnectedEventsViaSourcePool_WhenIndexIsCalled_ThenViewResultIsReturnedWithNonDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .Without(x => x.BankAccount)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent> { new() })
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, false, true))
                .Returns(expectedPools)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as PoolsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Pools, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Pools[0].Id, Is.EqualTo(expectedPools[0].Id));
                Assert.That(viewModel.Pools[0].Name, Is.EqualTo(expectedPools[0].Name));
                Assert.That(viewModel.Pools[0].Deletable, Is.False);
            });
        }

        [Test]
        public void GivenCurrentUserHasOnePoolWithConnectedEventsViaDestinationPool_WhenIndexIsCalled_ThenViewResultIsReturnedWithNonDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .Without(x => x.BankAccount)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent> { new() })
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, false, true))
                .Returns(expectedPools)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as PoolsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Pools, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Pools[0].Id, Is.EqualTo(expectedPools[0].Id));
                Assert.That(viewModel.Pools[0].Name, Is.EqualTo(expectedPools[0].Name));
                Assert.That(viewModel.Pools[0].Deletable, Is.False);
            });
        }

        [Test]
        public void GivenCurrentUserHasOnePoolWithNoConnectedEvents_WhenIndexIsCalled_ThenViewResultIsReturnedWithDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .Without(x => x.BankAccount)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent> { new() })
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, false, true))
                .Returns(expectedPools)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as PoolsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Pools, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Pools[0].Id, Is.EqualTo(expectedPools[0].Id));
                Assert.That(viewModel.Pools[0].Name, Is.EqualTo(expectedPools[0].Name));
                Assert.That(viewModel.Pools[0].Deletable, Is.True);
            });
        }
    }
}