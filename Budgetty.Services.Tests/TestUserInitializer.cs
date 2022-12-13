using Budgetty.Persistance;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Budgetty.Services.Tests
{
    [TestFixture]
    public class TestUserInitializer : TestBase<UserInitializer>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public async Task GivenUser_WhenInitializeUser_ThenUserHasTheirSequenceNumberAndShapshotLockInitialized()
        {
            // Given
            var user = CreateObject<IdentityUser>();

            // When
            await ClassUnderTest.InitializeUser(user);

            // Then
            GetMock<ISequenceNumberProvider>().Verify(x => x.InitialiseSequenceNumberAsync(user.Id), Times.Once);
            GetMock<ISnapshotLockManager>().Verify(x => x.InitialiseLock(user.Id), Times.Once);
        }
    }
}