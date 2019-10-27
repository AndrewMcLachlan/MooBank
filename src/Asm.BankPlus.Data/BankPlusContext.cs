using System;
using Asm.BankPlus.Data.Entities;
using Asm.BankPlus.Data.Entities.Ing;
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

        public virtual DbSet<TransactionTag> TransactionTags { get; set; }

        public virtual DbSet<TransactionTagRule> TransactionTagRules { get; set; }

        public virtual DbSet<VirtualAccount> VirtualAccounts { get; set; }

        public virtual DbSet<ImportAccount> ImportAccounts { get; set; }

        public virtual DbSet<ImporterType> ImporterTypes { get; set; }

        public virtual DbSet<TransactionExtra> TransactionsExtra {get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.ClrType.Name;
            }

            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.AccountId).ValueGeneratedOnAdd();
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

                entity.Property(r => r.AccountController)
                .HasConversion(e => (int)e, e => (Models.AccountController)e);
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

                entity.HasKey("TransactionId");

                entity.Property(e => e.TransactionId).ValueGeneratedOnAdd();

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

                entity.HasMany(p => p.TransactionTagLinks)
                    .WithOne(t => t.Transaction)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Transaction_TransactionTransactionTag");


                entity.Property(e => e.TransactionType)
                    .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
                    .HasConversion(e => (int)e, e => (Models.TransactionType)e)
                    .HasDefaultValue(Models.TransactionType.Debit);

            });

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

            modelBuilder.Entity<TransactionTag>(entity =>
            {
                entity.ToTable("TransactionTag");

                entity.Property(e => e.TransactionTagId).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasMany(d => d.TagLinks)
                    .WithOne(p => p.Primary)
                    .HasForeignKey(d => d.PrimaryTransactionTagId)
                    .HasConstraintName("FK_TransactionTag_TransactionTag_Primary");

                entity.HasMany(d => d.TaggedToLinks)
                    .WithOne(p => p.Secondary)
                    .HasForeignKey(d => d.SecondaryTransactionTagId)
                    .HasConstraintName("FK_TransactionTag_TransactionTag_Secondary");
            });

            modelBuilder.Entity<TransactionTagRule>(entity =>
            {
                entity.HasKey(t => t.TransactionTagRuleId);

                entity.Property(e => e.TransactionTagRuleId).ValueGeneratedOnAdd();

                entity.Property(e => e.Contains)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasMany(p => p.TransactionTagLinks)
                    .WithOne(t => t.TransactionTagRule)
                    .HasForeignKey(d => d.TransactionTagRuleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TransactionTagRule_TransactionTagRuleTransactionTag");

                entity.HasOne(e => e.Account)
                    .WithMany()
                    .HasForeignKey(e => e.AccountId);

            });

            modelBuilder.Entity<TransactionTagTransactionTag>(entity =>
            {
                entity.HasKey(e => new { e.PrimaryTransactionTagId, e.SecondaryTransactionTagId });

                /*entity.HasOne(d => d.Primary)
                    .WithMany(p => p.TransactionTagTransactionTagPrimaryTransactionTag)
                    .HasForeignKey(d => d.PrimaryTransactionTagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTag_TransactionTag_Primary");

                entity.HasOne(d => d.Secondary)
                    .WithMany(p => p.TransactionTagTransactionTagSecondaryTransactionTag)
                    .HasForeignKey(d => d.SecondaryTransactionTagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTag_TransactionTag_Secondary");*/
            });

            modelBuilder.Entity<TransactionTransactionTag>(entity =>
            {
                entity.HasKey(e => new { e.TransactionId, e.TransactionTagId });

                /*entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.TransactionTransactionTag)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTransactionTag_Transaction");

                entity.HasOne(d => d.TransactionTag)
                    .WithMany(p => p.TransactionTransactionTag)
                    .HasForeignKey(d => d.TransactionTagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionTransactionTag_TransactionTag");*/
            });

            modelBuilder.Entity<TransactionTagRuleTransactionTag>(entity =>
            {
                entity.HasKey(e => new { e.TransactionTagRuleId, e.TransactionTagId });
            });

            modelBuilder.Entity<ImportAccount>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.HasOne(e => e.Account).WithOne().HasForeignKey<Account>(a => a.AccountId);
                entity.HasOne(e => e.ImporterType).WithMany().HasForeignKey(i => i.ImporterTypeId);
            });

            modelBuilder.Entity<ImporterType>(entity =>
            {
                entity.HasKey(e => e.ImporterTypeId);
            });

            DefineIngModel(modelBuilder);
        }

        private void DefineIngModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionExtra>(entity =>
            {
                entity.ToTable("TransactionExtra", "ing");

                entity.HasKey(e => e.TransactionId);

                entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<Transaction>(e => e.TransactionId);
            });
        }
    }
}
