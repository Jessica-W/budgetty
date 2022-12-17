using Budgetty.Mvc.Controllers;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using Budgetty.Domain;
using Budgetty.Mvc.Models.BankAccounts;
using Budgetty.Persistance.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Budgetty.Mvc.Tests.Controllers
{
    [TestFixture]
    public class TestBankAccountsController : TestBase<BankAccountsController>
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
        public void GivenUserId_WhenIndexIsCalled_ThenViewResultIsReturnedWithCorrectViewModelValues()
        {
            // Given
            var bankAccounts = new List<BankAccount>
            {
                new ()
                {
                    Id = 1,
                    UserId = UserId,
                    Name = "Test Account A",
                },
                new ()
                {
                    Id = 2,
                    UserId = UserId,
                    Name = "Test Account B",
                },
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountsForUser(UserId, true))
                .Returns(bankAccounts)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as BankAccountsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.BankAccounts, Has.Count.EqualTo(2));
                Assert.That(viewModel.BankAccounts[0].Id, Is.EqualTo(bankAccounts[0].Id));
                Assert.That(viewModel.BankAccounts[0].Name, Is.EqualTo(bankAccounts[0].Name));
                Assert.That(viewModel.BankAccounts[0].Deletable, Is.True);
                Assert.That(viewModel.BankAccounts[1].Id, Is.EqualTo(bankAccounts[1].Id));
                Assert.That(viewModel.BankAccounts[1].Name, Is.EqualTo(bankAccounts[1].Name));
                Assert.That(viewModel.BankAccounts[1].Deletable, Is.True);
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenBankAccountHasAssociatedPools_WhenIndexIsCalled_ThenDeletableIsFalse()
        {
            // Given
            var pool = new BudgetaryPool();

            var bankAccounts = new List<BankAccount>
            {
                new ()
                {
                    Id = 1,
                    UserId = UserId,
                    Name = "Test Account A",
                    BudgetaryPools = new List<BudgetaryPool> { pool },
                },
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountsForUser(UserId, true))
                .Returns(bankAccounts)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as BankAccountsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.BankAccounts, Has.Count.EqualTo(1));
                Assert.That(viewModel.BankAccounts[0].Deletable, Is.False);
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        [Test]
        public void GivenBankAccountHasNoAssociatedPools_WhenIndexIsCalled_ThenDeletableIsTrue()
        {
            // Given
            var bankAccounts = new List<BankAccount>
            {
                new ()
                {
                    Id = 1,
                    UserId = UserId,
                    Name = "Test Account A",
                    BudgetaryPools = new List<BudgetaryPool>(),
                },
            };

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.GetBankAccountsForUser(UserId, true))
                .Returns(bankAccounts)
                .Verifiable();

            // When
            var result = ClassUnderTest.Index() as ViewResult;

            // Then
            Assert.That(result, Is.Not.Null);
            var viewModel = result!.Model as BankAccountsViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(viewModel!.BankAccounts, Has.Count.EqualTo(1));
                Assert.That(viewModel.BankAccounts[0].Deletable, Is.True);
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        #endregion

        #region Delete

        [Test]
        public void GivenBankAccountId_WhenDeleteIsCalled_ThenBudgetaryRepositoryIsUsedToDeleteTheBankAccountAndRedirectToIndexIsReturned()
        {
            // Given
            const int bankAccountId = 1;
            var deleteBankAccountCalled = false;

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.DeleteBankAccount(UserId, bankAccountId))
                .Callback(() => deleteBankAccountCalled = true)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.SaveChanges())
                .Callback(() =>
                    {
                        if (!deleteBankAccountCalled)
                        {
                            Assert.Fail($"SaveChanges called on repository before {nameof(IBudgetaryRepository.DeleteBankAccount)}");
                        }
                    }
                )
                .Verifiable();

            // When
            var result = ClassUnderTest.Delete(bankAccountId) as RedirectToActionResult;

            // Then
            Assert.That(result, Is.Not.Null);
            Assume.That(result!.ActionName, Is.EqualTo(nameof(BankAccountsController.Index)));

            GetMock<IBudgetaryRepository>().Verify();
        }

        #endregion

        #region CreateBankAccount

        [Test]
        public void GivenValidBankAccountDetails_WhenCreateBankAccountIsCalled_ThenBankAccountIsCreatedAndRedirectToIndexIsReturned()
        {
            // Given
            const string name = "Savings Account";

            var createBankAccountCalled = false;

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.CreateBankAccount(UserId, name))
                .Callback(() => createBankAccountCalled = true)
                .Verifiable();

            GetMock<IBudgetaryRepository>()
                .Setup(x => x.SaveChanges())
                .Callback(() =>
                {
                    if (!createBankAccountCalled)
                    {
                        Assert.Fail($"SaveChanges called before {nameof(IBudgetaryRepository.CreateBankAccount)}");
                    }
                })
                .Verifiable();

            // When
            var result = ClassUnderTest.CreateBankAccount(name) as RedirectToActionResult;

            // Then
            AssertRedirectsToIndex(result);

            GetMock<IBudgetaryRepository>().Verify();
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase("     ")]
        [TestCase(null)]
        public void GivenNameIsNullOrWhitespace_WhenCreateBankAccountIsCalled_ThenBankAccountIsCreatedAndRedirectToIndexIsReturned(string name)
        {
            // Given / When / Then
            var ex = Assert.Throws<ArgumentException>(() => { ClassUnderTest.CreateBankAccount(name); });
            Assert.That(ex!.Message, Is.EqualTo("Name must be provided (Parameter 'name')"));
        }

        #endregion

        private static void AssertRedirectsToIndex(RedirectToActionResult? result)
        {
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.ControllerName, Is.Null);
                Assert.That(result.ActionName, Is.EqualTo(nameof(PoolsController.Index)));
            });
        }
    }
}