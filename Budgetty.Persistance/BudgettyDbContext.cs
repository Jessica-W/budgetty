using Budgetty.Domain;
using Budgetty.Domain.BudgetaryEvents;
using Microsoft.EntityFrameworkCore;

namespace Budgetty.Persistance
{
    public class BudgettyDbContext : DbContext
    {
        public DbSet<BudgetaryEvent> BudgetaryEvents { get; set; } = null!;
        public DbSet<BudgetaryPool> BudgetaryPools { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<FinancialsSnapshot> FinancialsSnapshots { get; set; } = null!;
        public DbSet<SequenceNumber> SequenceNumbers { get; set; } = null!;
        public DbSet<SnapshotLock> SnapshotLocks { get; set; } = null!;

        private const string ConnectionString = "server=10.10.3.159;port=3307;database=Budgetty;user=budgetty;password=6SmilingSausagesAteTheBeans!";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .HasIndex(x => x.SequenceNumber)
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