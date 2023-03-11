﻿using System;
using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Data.Entities;
using Asm.MooBank.Data.Entities.Ing;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure
{
    public partial class BankPlusContext : DbContext
    {
        public BankPlusContext()
        {
        }

        public BankPlusContext(DbContextOptions<BankPlusContext> options) : base(options)
        {
        }

        [AllowNull]
        public virtual DbSet<Account> Accounts { get; set; }

        [AllowNull]
        public virtual DbSet<AccountAccountHolder> AccountAccountHolder { get; set; }

        [AllowNull]
        public virtual DbSet<AccountHolder> AccountHolders { get; set; }

        [AllowNull]
        public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }

        [AllowNull]
        public virtual DbSet<Transaction> Transactions { get; set; }

        [AllowNull]
        public virtual DbSet<TransactionTag> TransactionTags { get; set; }

        [AllowNull]
        public virtual DbSet<TransactionTagRule> TransactionTagRules { get; set; }

        [AllowNull]
        public virtual DbSet<VirtualAccount> VirtualAccounts { get; set; }

        [AllowNull]
        public virtual DbSet<ImportAccount> ImportAccounts { get; set; }

        [AllowNull]
        public virtual DbSet<ImporterType> ImporterTypes { get; set; }

        [AllowNull]
        public virtual DbSet<TransactionExtra> TransactionsExtra {get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.ClrType.Name);
            }

            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);


            modelBuilder.Entity<AccountAccountHolder>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.AccountHolderId });
            });

            modelBuilder.Entity<AccountHolder>(entity =>
            {
                entity.HasIndex(e => e.EmailAddress)
                    .HasDatabaseName("IX_AccountHolder_Email")
                    .IsUnique();

                entity.Property(e => e.AccountHolderId);

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

            /*modelBuilder.Entity<RecurringTransaction>(entity =>
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
                    .HasConstraintName("FK_RecurringTransaction_Schedule");* /

                entity.HasOne(d => d.SourceVirtualAccount)
                    .WithMany(p => p.RecurringTransactionSourceVirtualAccount)
                    .HasForeignKey(d => d.SourceVirtualAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RecurringTransaction_VirtualAccount_Source");
            });*/

            /*modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.ScheduleId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });*/


            modelBuilder.Entity<TransactionTag>(entity =>
            {
                entity.ToTable("TransactionTag");

                entity.Property(e => e.TransactionTagId).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasMany(d => d.Tags)
                      .WithMany(d => d.TaggedTo)
                      .UsingEntity<TransactionTagTransactionTag>(
                        t4 => t4.HasOne(t42 => t42.Primary)
                                  .WithMany()
                                  .HasForeignKey(t42 => t42.PrimaryTransactionTagId),
                        t4 => t4.HasOne(ttt2 => ttt2.Secondary)
                                  .WithMany()
                                  .HasForeignKey(ttt2 => ttt2.SecondaryTransactionTagId),
                        t4 =>
                        {
                            t4.HasKey(e => new { e.PrimaryTransactionTagId, e.SecondaryTransactionTagId });
                        });
            });

            modelBuilder.Entity<TransactionTagRule>(entity =>
            {
                entity.HasKey(t => t.TransactionTagRuleId);

                entity.Property(e => e.TransactionTagRuleId).ValueGeneratedOnAdd();

                entity.Property(e => e.Contains)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasMany(p => p.TransactionTags)
                   .WithMany(d => d.Rules)
                      .UsingEntity<TransactionTagRuleTransactionTag>(
                        ttr => ttr.HasOne(ttr2 => ttr2.TransactionTag)
                                  .WithMany()
                                  .HasForeignKey(tt2 => tt2.TransactionTagId),
                        ttr => ttr.HasOne(ttr2 => ttr2.TransactionTagRule)
                                  .WithMany()
                                  .HasForeignKey(ttr2 => ttr2.TransactionTagRuleId),
                        t4 =>
                        {
                            t4.HasKey(e => new { e.TransactionTagRuleId, e.TransactionTagId });
                        });

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

            modelBuilder.Entity<TransactionTagRuleTransactionTag>(entity =>
            {
                entity.HasKey(e => new { e.TransactionTagRuleId, e.TransactionTagId });
            });


            modelBuilder.Entity<ImporterType>(entity =>
            {
                entity.HasKey(e => e.ImporterTypeId);
            });

        }
    }
}