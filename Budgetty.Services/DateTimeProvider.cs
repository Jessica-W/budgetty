using System.Diagnostics.CodeAnalysis;
using Budgetty.Services.Interfaces;

namespace Budgetty.Services;

[ExcludeFromCodeCoverage]
internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;

    public DateOnly GetDateNow() => DateOnly.FromDateTime(DateTime.UtcNow);
}