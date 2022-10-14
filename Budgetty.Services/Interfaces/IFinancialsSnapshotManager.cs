using Budgetty.Domain;

namespace Budgetty.Services.Interfaces;

public interface IFinancialsSnapshotManager
{
    Task CreateSnapshotAsync(string userId, FinancialState state, DateOnly date);
    Task<FinancialsSnapshot?> GetSnapshotAsync(string userId);
}