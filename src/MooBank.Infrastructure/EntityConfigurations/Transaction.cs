using System.Text.Json;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        // Required do to computed column savings issues. See https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes#sqlserver-tables-with-triggers
        entity.ToTable(tb => tb.UseSqlOutputClause(false));

        //entity.HasKey("Id");
        entity.Property(t => t.Id).HasColumnName("TransactionId");

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Transactions)
            .HasForeignKey(d => d.AccountId);

        entity.HasMany(p => p.Splits)
            .WithOne(s => s.Transaction).HasForeignKey(e => e.TransactionId);

        entity.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (TransactionType)e)
            .HasDefaultValue(TransactionType.Debit)
            .HasSentinel(TransactionType.NotSet);

        entity.Property(e => e.TransactionSubType)
            .HasColumnName($"{nameof(Transaction.TransactionSubType)}Id")
            .HasConversion(e => (int?)e, e => (TransactionSubType?)e)
            .HasSentinel(null);

        // This transaction offsets the linked "TransactionId" transaction
        entity.HasMany(e => e.OffsetFor).WithOne(e => e.OffsetByTransaction).HasForeignKey(t => t.OffsetTransactionId);

        // Extra is a polymorphic payload written by the institution importers (each has its own
        // TransactionExtra type), so it cannot be deserialized to a single concrete type.
        // The string-snapshot value comparer ensures EF change detection works for the mutable
        // JsonElement-based values produced on materialisation.
        entity.Property(e => e.Extra).HasConversion(
                       v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                       v => JsonSerializer.Deserialize<object>(v, JsonSerializerOptions.Default),
                       new ValueComparer<object?>(
                           (l, r) => JsonSerializer.Serialize(l, JsonSerializerOptions.Default) == JsonSerializer.Serialize(r, JsonSerializerOptions.Default),
                           v => v == null ? 0 : JsonSerializer.Serialize(v, JsonSerializerOptions.Default).GetHashCode(),
                           v => v == null ? null : JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(v, JsonSerializerOptions.Default), JsonSerializerOptions.Default)));
    }
}
