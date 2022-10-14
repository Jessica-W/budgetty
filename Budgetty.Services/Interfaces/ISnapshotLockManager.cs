namespace Budgetty.Services.Interfaces;

public interface ISnapshotLockManager
{
    Task InitialiseLock(string userId);
    Task<bool> TryGetLockAsync(string userId);
    Task ReleaseLockAsync(string userId);
}