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
                .HasDiscriminator<string>("EventType")
                .HasValue<ExpenditureEvent>("expenditure_event")
                .HasValue<IncomeAllocationEvent>("income_allocation_event")
                .HasValue<IncomeEvent>("income_event")
                .HasValue<PoolTransferEvent>("pool_transfer_event");
            
            modelBuilder.Entity<IncomeEvent>()
                .HasBaseType<BudgetaryEvent>();

            modelBuilder.Entity<IncomeAllocationEvent>()
                .HasBaseType<BudgetaryEvent>();

            modelBuilder.Entity<ExpenditureEvent>()
                .HasBaseType<BudgetaryEvent>();

            modelBuilder.Entity<PoolTransferEvent>()
                .HasBaseType<BudgetaryEvent>();

            modelBuilder.Entity<ExpenditureEvent>()
                .HasIndex(x => new { x.UserId, x.SequenceNumber })
                .IsUnique();

            modelBuilder.Entity<ExpenditureEvent>()
                .Property(x => x.Description);

            modelBuilder.Entity<ExpenditureEvent>()
                .Property(x => x.AmountInPennies)
                .HasColumnName("AmountInPennies");

            modelBuilder.Entity<IncomeAllocationEvent>()
                .Property(x => x.AmountInPennies)
                .HasColumnName("AmountInPennies");

            modelBuilder.Entity<IncomeEvent>()
                .Property(x => x.AmountInPennies)
                .HasColumnName("AmountInPennies");

            modelBuilder.Entity<PoolTransferEvent>()
                .Property(x => x.AmountInPennies)
                .HasColumnName("AmountInPennies");

            modelBuilder.Entity<ExpenditureEvent>()
                .HasOne(x => x.Pool)
                .WithMany()
                .IsRequired();
        }
    }
}