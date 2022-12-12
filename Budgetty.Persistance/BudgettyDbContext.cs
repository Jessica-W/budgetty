using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Persistance
{
    public class BudgettyDbContext : IdentityDbContext
    {
        public DbSet<BudgetaryEvent> BudgetaryEvents { get; set; } = null!;
        public DbSet<BudgetaryPool> BudgetaryPools { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<FinancialsSnapshot> FinancialsSnapshots { get; set; } = null!;
        public DbSet<SequenceNumber> SequenceNumbers { get; set; } = null!;
        public DbSet<SnapshotLock> SnapshotLocks { get; set; } = null!;

        public BudgettyDbContext(DbContextOptions<BudgettyDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BudgetaryEvent>()
                .HasOne(x => x.DestinationPool)
                .WithMany(x => x.BudgetaryEventsAsDestination);

            modelBuilder.Entity<BudgetaryEvent>()
                .HasOne(x => x.SourcePool)
                .WithMany(x => x.BudgetaryEventsAsSource);

            modelBuilder.Entity<BudgetaryPool>()
                .HasOne(x => x.BankAccount)
                .WithMany(x => x.BudgetaryPools);

            modelBuilder.Entity<BudgetaryPool>()
                .Ignore(x => x.BudgetaryEvents);
        }
    }
}