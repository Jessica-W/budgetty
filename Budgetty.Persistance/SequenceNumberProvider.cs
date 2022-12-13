using System.Diagnostics.CodeAnalysis;
using Budgetty.Domain;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Persistance
{
    [ExcludeFromCodeCoverage] // Impossible to unit test
    internal class SequenceNumberProvider : ISequenceNumberProvider
    {
        private readonly IDbContextFactory<BudgettyDbContext> _budgettyDbContextFactory;

        public SequenceNumberProvider(IDbContextFactory<BudgettyDbContext> budgettyDbContextFactory)
        {
            _budgettyDbContextFactory = budgettyDbContextFactory;
        }

        public int GetNextSequenceNumber(string userId)
        {
            using var budgettyDbContext = _budgettyDbContextFactory.CreateDbContext();

            var t1 = budgettyDbContext.Database.BeginTransaction();
            var sn = budgettyDbContext.SequenceNumbers.FromSqlInterpolated($"SELECT * FROM SequenceNumbers WHERE UserId={userId} FOR UPDATE;").ToList().First();
            budgettyDbContext.Database.ExecuteSqlInterpolated($"UPDATE SequenceNumbers SET SequenceNo = SequenceNo + 1 WHERE UserId={userId};");
            t1.Commit();
            
            var seqNo = sn.SequenceNo;

            return seqNo;
        }

        public async Task InitialiseSequenceNumberAsync(string userId)
        {
            await using var budgettyDbContext = await _budgettyDbContextFactory.CreateDbContextAsync();

            var sequenceNumber = new SequenceNumber
            {
                SequenceNo = 1,
                UserId = userId,
            };
            
            await budgettyDbContext.SequenceNumbers.AddAsync(sequenceNumber);
            await budgettyDbContext.SaveChangesAsync();
        }
    }
}