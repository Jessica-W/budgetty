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

        public int GetNextSequenceNumber()
        {
            var t1 = _budgettyDbContext.Database.BeginTransaction();
            var sn = _budgettyDbContext.SequenceNumbers.FromSqlRaw("SELECT * FROM SequenceNumbers WHERE Id=1 FOR UPDATE;").ToList().First();
            _budgettyDbContext.Database.ExecuteSqlRaw("UPDATE SequenceNumbers SET SequenceNo = SequenceNo + 1 WHERE Id=1;");
            t1.Commit();

            var seqNo = sn.SequenceNo;
            return seqNo;
        }
    }
}