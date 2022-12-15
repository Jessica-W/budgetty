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

        #region Delete
        
        [Test]
        public void
            GivenPoolIdAndCurrentUser_WhenDeleteIsCalled_ThenBudgetaryRepositoryIsUsedToDeleteThePoolAndRedirectToIndexIsReturned()
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
                .Callback(() =>
                {
                    if (!deletePoolCalled)
                    {
                        Assert.Fail("SaveChanges called on repository before pool was deleted");
                    }
                })
                .Verifiable();

            // When
            var result = ClassUnderTest.Delete(poolId) as RedirectToActionResult;

            // Then
            AssertRedirectsToIndex(result);

            GetMock<IBudgetaryRepository>().Verify();
            GetMock<IBudgetaryRepository>().Verify();
        }

        #endregion

        #region Index

        [Test]
        public void
            GivenCurrentUserHasOnePoolWithConnectedEventsViaSourcePool_WhenIndexIsCalled_ThenViewResultIsReturnedWithNonDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Income)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent> { new() })
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, true, true))
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
                Assert.That(viewModel.Pools[0].Deletable, Is.False);
            });
        }

        [Test]
        public void
            GivenCurrentUserHasOnePoolWithConnectedEventsViaDestinationPool_WhenIndexIsCalled_ThenViewResultIsReturnedWithNonDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Income)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent> { new() })
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, true, true))
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
                Assert.That(viewModel.Pools[0].Deletable, Is.False);
            });
        }

        [Test]
        public void
            GivenCurrentUserHasOnePoolWithNoConnectedEvents_WhenIndexIsCalled_ThenViewResultIsReturnedWithDeletablePoolInViewModel()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Income)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, true, true))
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
                Assert.That(viewModel.Pools[0].Deletable, Is.True);
            });
        }

        [Test]
        public void
            GivenCurrentUserHasAnIncomePool_WhenIndexIsCalled_ThenViewResultIsReturnedWithCorrectValues()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Income)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Income)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
            }
                .OrderBy(x => x.BankAccount?.Name ?? "N/A")
                .ToList();

            var bankAccounts = new List<BankAccount>
            {
                new()
                {
                    Id = 1,
                    Name = "Account 1",
                },
                new()
                {
                    Id = 2,
                    Name = "Account 2",
                },
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, true, true))
                .Returns(expectedPools)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountsForUser(UserId))
                .Returns(bankAccounts);

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as PoolsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel!.Pools, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Pools[0].Id, Is.EqualTo(expectedPools[0].Id));
                Assert.That(viewModel.Pools[0].Name, Is.EqualTo(expectedPools[0].Name));
                Assert.That(viewModel.Pools[0].BankAccountName, Is.EqualTo(expectedPools[0].BankAccount!.Name));
                Assert.That(viewModel.Pools[0].Type, Is.EqualTo(expectedPools[0].Type.ToString()));
                Assert.That(viewModel.Pools[0].Deletable, Is.True);

                Assert.That(viewModel.Pools[1].Id, Is.EqualTo(expectedPools[1].Id));
                Assert.That(viewModel.Pools[1].Name, Is.EqualTo(expectedPools[1].Name));
                Assert.That(viewModel.Pools[1].BankAccountName, Is.EqualTo(expectedPools[1].BankAccount!.Name));
                Assert.That(viewModel.Pools[1].Type, Is.EqualTo(expectedPools[1].Type.ToString()));
                Assert.That(viewModel.Pools[1].Deletable, Is.True);

                Assert.That(viewModel.AvailableBankAccounts, Has.Count.EqualTo(2));
                Assert.That(viewModel.AvailableBankAccounts[0].Id, Is.EqualTo(bankAccounts[0].Id));
                Assert.That(viewModel.AvailableBankAccounts[0].Name, Is.EqualTo(bankAccounts[0].Name));
                Assert.That(viewModel.AvailableBankAccounts[1].Id, Is.EqualTo(bankAccounts[1].Id));
                Assert.That(viewModel.AvailableBankAccounts[1].Name, Is.EqualTo(bankAccounts[1].Name));
            });
        }

        [Test]
        public void
            GivenCurrentUserHasADebtPool_WhenIndexIsCalled_ThenViewResultIsReturnedWithNameAndNotApplicableAsBankAccountName()
        {
            // Given
            var expectedPools = new List<BudgetaryPool>
            {
                BuildObject<BudgetaryPool>()
                    .With(x => x.Type, PoolType.Debt)
                    .Without(x => x.BankAccount)
                    .With(x => x.BudgetaryEventsAsDestination, new List<BudgetaryEvent>())
                    .With(x => x.BudgetaryEventsAsSource, new List<BudgetaryEvent>())
                    .Create(),
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBudgetaryPoolsForUser(UserId, true, true))
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
                Assert.That(viewModel.Pools[0].BankAccountName, Is.EqualTo("N/A"));
                Assert.That(viewModel.Pools[0].Deletable, Is.True);
            });
        }

        #endregion

        #region CreatePool

        [Test]
        public void GivenValidPoolDetails_WhenCreatePoolIsCalled_ThenPoolIsCreatedAndRedirectToIndexIsReturned()
        {
            // Given
            const string poolName = "pool";
            const PoolType poolType = PoolType.Income;
            const int bankAccountId = 1;

            var bankAccount = new BankAccount
            {
                Id = bankAccountId,
                UserId = UserId,
                Name = "account",
            };

            var createBudgetaryPoolAccount = false;

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountForUser(UserId, bankAccountId))
                .Returns(bankAccount)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.CreateBudgetaryPoolAccount(UserId, poolName, poolType, bankAccount))
                .Callback(() => createBudgetaryPoolAccount = true)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.SaveChanges())
                .Callback(() =>
                {
                    if (!createBudgetaryPoolAccount)
                    {
                        Assert.Fail($"SaveChanges called before {nameof(IBudgetaryRepository.CreateBudgetaryPoolAccount)}");
                    }
                })
                .Verifiable();

            // When
            var result = ClassUnderTest.CreatePool(poolName, poolType, bankAccountId) as RedirectToActionResult;

            // Then
            AssertRedirectsToIndex(result);

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenValidPoolDetailsAndPoolTypeIsDebt_WhenCreatePoolIsCalled_ThenPoolIsCreatedAndBankAccountIdIsNotUsedAndRedirectToIndexIsReturned()
        {
            // Given
            const string poolName = "pool";
            const PoolType poolType = PoolType.Debt;
            const int bankAccountId = 1;
            
            var createBudgetaryPoolAccount = false;
            
            GetMock<IBudgetaryRepository>()
                .Setup(x => x.CreateBudgetaryPoolAccount(UserId, poolName, poolType, null))
                .Callback(() => createBudgetaryPoolAccount = true)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.SaveChanges())
                .Callback(() =>
                {
                    if (!createBudgetaryPoolAccount)
                    {
                        Assert.Fail($"SaveChanges called before {nameof(IBudgetaryRepository.CreateBudgetaryPoolAccount)}");
                    }
                })
                .Verifiable();

            // When
            var result = ClassUnderTest.CreatePool(poolName, poolType, bankAccountId) as RedirectToActionResult;

            // Then
            AssertRedirectsToIndex(result);

            GetMock<IBudgetaryRepository>().Verify();
            GetMock<IBudgetaryRepository>().Verify(x => x.GetBankAccountForUser(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase("     ")]
        [TestCase(null)]
        public void GivenNameIsWhitespaceOrEmpty_WhenCreatePoolIsCalled_ThenArgumentExceptionIsThrown(string poolName)
        {
            // Given
            const PoolType poolType = PoolType.Income;
            const int bankAccountId = 1;

            var bankAccount = new BankAccount
            {
                Id = bankAccountId,
                UserId = UserId,
                Name = "account",
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountForUser(UserId, bankAccountId))
                .Returns(bankAccount)
                .Verifiable();

            // When
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.CreatePool(poolName, poolType, bankAccountId));
            Assert.That(ex!.Message, Is.EqualTo("Name must be provided (Parameter 'name')"));
        }

        [Test]
        public void GivenPoolTypeHasInvalidValue_WhenCreatePoolIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const string poolName = "pool";
            const PoolType poolType = (PoolType)(-1);
            const int bankAccountId = 1;

            var bankAccount = new BankAccount
            {
                Id = bankAccountId,
                UserId = UserId,
                Name = "account",
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountForUser(UserId, bankAccountId))
                .Returns(bankAccount)
                .Verifiable();
            
            // When
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.CreatePool(poolName, poolType, bankAccountId));
            Assert.That(ex!.Message, Is.EqualTo("Invalid pool type (Parameter 'poolType')"));
        }

        [Test]
        public void GivenPoolTypeIsIncomeAndBankAccountIdMatchesNoBankAccount_WhenCreatePoolIsCalled_ThenArgumentExceptionIsThrown()
        {
            // Given
            const string poolName = "pool";
            const PoolType poolType = PoolType.Income;
            const int bankAccountId = 1;
            
            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountForUser(UserId, bankAccountId))
                .Returns((BankAccount?)null)
                .Verifiable();
            
            // When
            var ex = Assert.Throws<ArgumentException>(() => ClassUnderTest.CreatePool(poolName, poolType, bankAccountId));
            Assert.That(ex!.Message, Is.EqualTo("Invalid bank account ID (Parameter 'bankAccountId')"));
        }

        private static void AssertRedirectsToIndex(RedirectToActionResult? result)
        {
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.ControllerName, Is.Null);
                Assert.That(result.ActionName, Is.EqualTo(nameof(PoolsController.Index)));
            });
        }

        #endregion
    }
}