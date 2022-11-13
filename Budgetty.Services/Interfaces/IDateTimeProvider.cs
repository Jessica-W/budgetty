namespace Budgetty.Services.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime GetUtcNow();
        DateOnly GetDateNow();
    }
}