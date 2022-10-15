namespace Budgetty.Persistance
{
    public interface ISequenceNumberProvider
    {
        int GetNextSequenceNumber(string userId);
        Task InitialiseSequenceNumberAsync(string userId);
    }
}