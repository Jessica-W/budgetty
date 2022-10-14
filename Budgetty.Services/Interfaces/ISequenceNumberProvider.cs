namespace Budgetty.Services.Interfaces;

public interface ISequenceNumberProvider
{
    int GetNextSequenceNumber(string userId);
    Task InitialiseSequenceNumberAsync(string userId);
}