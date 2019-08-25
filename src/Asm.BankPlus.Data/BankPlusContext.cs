using Asm.BankPlus.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Data
{
    public partial class BankPlusContext : DbContext
    {
        public BankPlusContext()
        {
        }

        public BankPlusContext(DbContextOptions<BankPlusContext> options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountAccountHolder> AccountAccountHolder { get; set; }
        public virtual DbSet<AccountHolder> AccountHolders { get; set; }

        public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }

        public virtual DbSet<Transaction> Transactions { get; set; }

        public virtual DbSet<TransactionCategory> TransactionCategories { get; set; }

        public virtual DbSet<TransactionCategoryRule> TransactionCategoryRules { get; set; }

        public virtual DbSet<VirtualAccount> VirtualAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach(var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.ClrType.Name;
            }

            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountBalance).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.AvailableBalance).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("(sysutcdatetime())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.AccountType)
                .HasConversion(e => (int)e, e => (Models.AccountType)e);
            });

            modelBuilder.Entity<AccountAccountHolder>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.AccountHolderId });
            });

            modelBuilder.Entity<AccountHolder>(entity =>
            {
                entity.HasIndex(e => e.EmailAddress)
                    .HasName("IX_AccountHolder_Email")
                    .IsUnique();

                entity.Property(e => e.AccountHolderId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.DestinationVirtualAccount)
                    .WithMany(p => p.RecurringTransactionDestinationVirtualAccount)
                    .HasForeignKey(d => d.DestinationVirtualAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecurringTransaction_VirtualAccount_Destination");

                /*entity.HasOne(d => d.Schedule)
                    .WithMany(p => p.RecurringTransaction)
                    .HasForeignKey(d => d.ScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecurringTransaction_Schedule");*/

                entity.HasOne(d => d.SourceVirtualAccount)
                    .WithMany(p => p.RecurringTransactionSourceVirtualAccount)
                    .HasForeignKey(d => d.SourceVirtualAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecurringTransaction_VirtualAccount_Source");
            });

            /*modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.ScheduleId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });*/

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.TransactionId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionTime).HasDefaultValueSql("(sysdatetime())");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transaction_Account");

                entity.HasOne(d => d.TransactionCategory)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.TransactionCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transaction_TransactionCategory");

                /*entity.HasOne(d => d.TransactionType)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.TransactionTypeId)
                    .HasConstraintName("FK_Transaction_TransactionType");*/
            });

            /*modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.Property(e => e.TransactionTypeId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });*/

            modelBuilder.Entity<VirtualAccount>(entity =>
            {
                entity.HasIndex(e => e.DefaultAccount)
                    .IsUnique()
                    .HasFilter("([DefaultAccount]=(1))");

                entity.Property(e => e.VirtualAccountId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Balance).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });


            modelBuilder.Entity<TransactionCategory>(entity =>
            {
               // entity.ToTable("TransactionCategory");

                entity.Property(e => e.TransactionCategoryId).ValueGeneratedOnAdd();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentCategoryId)
                    .HasConstraintName("FK_TransactionCategory_TransactionCategory");
            });

            modelBuilder.Entity<TransactionCategoryRule>(entity =>
            {
                entity.Property(e => e.TransactionCategoryRuleId).ValueGeneratedOnAdd();

                entity.Property(e => e.Contains)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.TransactionCategory)
                    .WithMany(p => p.TransactionCategoryRules)
                    .HasForeignKey(d => d.TransactionCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionCategoryRule_TransactionCategory");
            });

        }
    }
}
