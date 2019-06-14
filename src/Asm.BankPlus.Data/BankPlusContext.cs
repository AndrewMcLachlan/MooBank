using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Asm.BankPlus.Data.Models;

namespace Asm.BankPlus.Data
{
    public partial class BankPlusContext : DbContext
    {
        static BankPlusContext()
        {
        }

        public BankPlusContext()
        {
        }

        public BankPlusContext(DbContextOptions<BankPlusContext> options) : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<RecurringTransaction> RecurringTransaction { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<TransactionType> TransactionType { get; set; }
        public virtual DbSet<VirtualAccount> VirtualAccount { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccountBalance).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.AvailableBalance).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("(sysdatetime())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateVirtualAccount)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
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

                /*entity.HasOne(d => d.Schedule.ToString())
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

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.ScheduleId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionTime).HasDefaultValueSql("(sysdatetime())");

                /*entity.HasOne(d => d.TransactionType.ToString())
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.TransactionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transaction_TransactionType");*/

                entity.HasOne(d => d.VirtualAccount)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.VirtualAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transaction_VirtualAccount");
            });

            modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.Property(e => e.TransactionTypeId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
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
        }
    }
}
