using Budgetty.Domain;

namespace Budgetty.Services.Interfaces;

public interface IFinancialsSnapshotManager
{
    Task CreateSnapshotAsync(FinancialState state, DateOnly date);
    Task<FinancialsSnapshot?> GetSnapshotAsync();
}