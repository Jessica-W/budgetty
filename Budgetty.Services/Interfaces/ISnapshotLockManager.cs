namespace Budgetty.Services.Interfaces;

public interface ISnapshotLockManager
{
    Task<bool> TryGetLockAsync();
    Task ReleaseLockAsync();
}