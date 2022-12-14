using Budgetty.Mvc.Filters;
using Budgetty.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Budgetty.Mvc.Tests.Filters
{
    [TestFixture]
    public class TestLogCsrfFailureFilter : TestBase<LogCsrfFailureFilter>
    {
        protected override void SetUp()
        {
        }

        [Test]
        public async Task GivenResultExecutingContextWithOkResultAndNextResultExecutionDelegate_WhenOnResultExecutionAsyncIsCalled_ThenNextResultExecutionDelegateIsCalled()
        {
            // Given
            var nextCalled = false;
            var context = CreateResultExecutingContext(new OkResult());

            // When
            await ClassUnderTest.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Then
            Assert.That(nextCalled, Is.True);
        }

        [Test]
        public void GivenResultExecutingContextWithOkResultAndNullNextResultExecutionDelegate_WhenOnResultExecutionAsyncIsCalled_ThenFilterHandlesNullDelegate()
        {
            // Given
            var context = CreateResultExecutingContext(new OkResult());

            // When / Then
            Assert.DoesNotThrowAsync(() => ClassUnderTest.OnResultExecutionAsync(context, null!));
        }

        [Test]
        public async Task GivenResultExecutingContextWithAntiforgeryValidationFailedResultAndNextResultExecutionDelegate_WhenOnResultExecutionAsyncIsCalled_ThenNextResultExecutionDelegateIsCalled()
        {
            // Given
            var nextCalled = false;
            var context = CreateResultExecutingContext(new AntiforgeryValidationFailedResult());

            GetMock<IServiceProvider>()
                .Setup(x => x.GetService(typeof(ILogger<LogCsrfFailureFilter>)))
                .Returns(GetMock<ILogger<LogCsrfFailureFilter>>().Object);

            // When
            await ClassUnderTest.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Then
            Assert.That(nextCalled, Is.True);
            //logger.Log(logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
            GetMock<ILogger<LogCsrfFailureFilter>>()
                .Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((y, type) => MatchesLogMessage(y, "Missing or invalid CSRF token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ));
        }

        private static bool MatchesLogMessage(object y, string expectedMessage)
        {
            return y is IReadOnlyList<KeyValuePair<string, object?>> { Count: 1 } list &&
                   list[0].Value as string == expectedMessage;
        }

        private ResultExecutingContext CreateResultExecutingContext(IActionResult actionResult)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = GetMock<IServiceProvider>().Object,
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary()
            );
            var actionExecutingContext =
                new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), actionResult,
                    null!);

            return actionExecutingContext;
        }
    }
}