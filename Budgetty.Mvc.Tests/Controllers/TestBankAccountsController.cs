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
                Assert.That(viewModel.BankAccounts[1].Id, Is.EqualTo(bankAccounts[1].Id));
                Assert.That(viewModel.BankAccounts[1].Name, Is.EqualTo(bankAccounts[1].Name));
            });

            GetMock<IBudgetaryRepository>().Verify();
        }

        #endregion
    }
}