using Budgetty.Mvc.Identity;
using Budgetty.Persistance;
using Budgetty.Persistance.Repositories;
using Budgetty.Services.Interfaces;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Budgetty.Mvc.Tests.Identity
{
    public class TestApplicationUserManager : TestBase<ApplicationUserManager>
    {
        protected override void SetUp()
        {
        }

        /*protected override ApplicationUserManager CreateDefaultClassUnderTest()
        {
            return new ApplicationUserManager(
                GetMock<IUserStore<IdentityUser>>().Object,
                GetMock<IOptions<IdentityOptions>>().Object,
                GetMock<IPasswordHasher<IdentityUser>>().Object,
                new List<IUserValidator<IdentityUser>>(),
                new List<IPasswordValidator<IdentityUser>>(),
                GetMock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                GetMock<IServiceProvider>().Object,
                GetMock<Logger<UserManager<IdentityUser>>>().Object,
                GetMock<ISequenceNumberProvider>().Object,
                GetMock<ISnapshotLockManager>().Object,
                GetMock<IBudgetaryRepository>().Object,
                GetMock<IBudgetaryEventFactory>().Object
            );
        }*/

        [Test]
        public async Task Given_When_Then()
        {
            // Given
            var user = new IdentityUser();

            // When
            var result = await ClassUnderTest.CreateAsync(user);

            // Then

        }
    }
}