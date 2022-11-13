namespace Budgetty.Services
{
    public static class DateHelper
    {
        public static DateOnly MonthStart(this DateOnly date)
        {
            return new DateOnly(date.Year, date.Month, 1);
        }

        public static DateOnly MonthEnd(this DateOnly date)
        {
            return date.MonthStart().AddMonths(1).AddDays(-1);
        }

        public static DateOnly Min(DateOnly dateA, DateOnly dateB)
        {
            return dateA < dateB ? dateA : dateB;
        }

        public static DateOnly Max(DateOnly dateA, DateOnly dateB)
        {
            return dateA > dateB ? dateA : dateB;
        }

        public static int DaysBetween(DateOnly dateA, DateOnly dateB)
        {
            var dateTimeA = new DateTime(dateA.Year, dateA.Month, dateA.Day);
            var dateTimeB = new DateTime(dateB.Year, dateB.Month, dateB.Day);

            return (dateTimeB - dateTimeA).Days;
        }
    }
}