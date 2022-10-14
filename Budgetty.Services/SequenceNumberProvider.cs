using Budgetty.Domain;
using Budgetty.Persistance;
using Budgetty.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Services
{
    public class SequenceNumberProvider : ISequenceNumberProvider
    {
        private readonly BudgettyDbContext _budgettyDbContext;

        public SequenceNumberProvider(BudgettyDbContext budgettyDbContext)
        {
            _budgettyDbContext = budgettyDbContext;
        }

        public int GetNextSequenceNumber(string userId)
        {
            var t1 = _budgettyDbContext.Database.BeginTransaction();
            var sn = _budgettyDbContext.SequenceNumbers.FromSqlInterpolated($"SELECT * FROM SequenceNumbers WHERE UserId={userId} FOR UPDATE;").ToList().First();
            _budgettyDbContext.Database.ExecuteSqlRaw("UPDATE SequenceNumbers SET SequenceNo = SequenceNo + 1 WHERE Id=1;");
            t1.Commit();

            var seqNo = sn.SequenceNo;
            return seqNo;
        }

        public async Task InitialiseSequenceNumberAsync(string userId)
        {
            var sequenceNumber = new SequenceNumber
            {
                SequenceNo = 1,
                UserId = userId,
            };

            // TODO: Figure out how to isolate this change to the DB
            await _budgettyDbContext.SequenceNumbers.AddAsync(sequenceNumber);
            await _budgettyDbContext.SaveChangesAsync();
        }
    }
}