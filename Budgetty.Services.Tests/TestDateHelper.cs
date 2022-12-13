namespace Budgetty.Services.Tests
{
    [TestFixture]
    public class TestDateHelper
    {
        [Test]
        public void GivenTwoDatesAndDateBIsLater_WhenMaxIsCalled_ThenDateBIsReturned()
        {
            // Given
            var dateA = new DateOnly(2022, 12, 13);
            var dateB = new DateOnly(2022, 12, 14);

            // When
            var result = DateHelper.Max(dateA, dateB);

            // Then
            Assert.That(result, Is.EqualTo(dateB));
        }

        [Test]
        public void GivenTwoDatesAndDateAIsLater_WhenMaxIsCalled_ThenDateAIsReturned()
        {
            // Given
            var dateA = new DateOnly(2022, 12, 14);
            var dateB = new DateOnly(2022, 12, 13);

            // When
            var result = DateHelper.Max(dateA, dateB);

            // Then
            Assert.That(result, Is.EqualTo(dateA));
        }

        [Test]
        public void GivenTwoDatesAndDateBIsEarlier_WhenMinIsCalled_ThenDateBIsReturned()
        {
            // Given
            var dateA = new DateOnly(2022, 12, 14);
            var dateB = new DateOnly(2022, 12, 13);

            // When
            var result = DateHelper.Min(dateA, dateB);

            // Then
            Assert.That(result, Is.EqualTo(dateB));
        }

        [Test]
        public void GivenTwoDatesAndDateAIsEarlier_WhenMinIsCalled_ThenDateAIsReturned()
        {
            // Given
            var dateA = new DateOnly(2022, 12, 13);
            var dateB = new DateOnly(2022, 12, 14);

            // When
            var result = DateHelper.Min(dateA, dateB);

            // Then
            Assert.That(result, Is.EqualTo(dateA));
        }

        [Test]
        public void GivenADateWhichIsNotAtTheStartOfTheMonth_WhenMonthStartIsCalled_ThenDateForTheStartOfTheMonthIsReturned()
        {
            // Given
            var date = new DateOnly(2022, 12, 13);
            var expected = new DateOnly(2022, 12, 1);

            // When
            var result = DateHelper.MonthStart(date);

            // Then
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(2022, 2, 28)]
        [TestCase(2020, 2, 29)]
        [TestCase(2100, 2, 28)]
        [TestCase(2021, 9, 30)]
        [TestCase(2019, 12, 31)]
        public void GivenADateWhichIsNotAtTheStartOfTheMonth_WhenMonthEndIsCalled_ThenDateForTheStartOfTheMonthIsReturned(int year, int month, int expectedDay)
        {
            // Given
            var date = new DateOnly(year, month, 10);
            var expected = new DateOnly(year, month, expectedDay);

            // When
            var result = DateHelper.MonthEnd(date);

            // Then
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(2022, 2, 10, 2022, 3, 1, 19)]
        [TestCase(2020, 2, 10, 2020, 3, 1, 20)]
        [TestCase(2020, 1, 1, 2021, 1, 1, 366)]
        [TestCase(2022, 1, 1, 2023, 1, 1, 365)]
        public void GivenTwoDates_WhenDaysBetweenIsCalled_ThenTheCorrectNumberOfDaysBetweenTheDatesIsReturned(int yearA, int monthA, int dayA, int yearB, int monthB, int dayB, int expectedDays)
        {
            // Given
            var dateA = new DateOnly(yearA, monthA, dayA);
            var dateB = new DateOnly(yearB, monthB, dayB);

            // When
            var result = DateHelper.DaysBetween(dateA, dateB);

            // Then
            Assert.That(result, Is.EqualTo(expectedDays));
        }
    }
}