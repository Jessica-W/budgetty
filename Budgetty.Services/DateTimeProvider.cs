using Budgetty.Services.Interfaces;

namespace Budgetty.Services;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;
    public DateOnly GetDateNow() => DateOnly.FromDateTime(DateTime.UtcNow);
}