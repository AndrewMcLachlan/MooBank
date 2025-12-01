using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.Macquarie.Domain;

[AggregateRoot]
internal class TransactionRaw([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public TransactionRaw() : this(Guid.NewGuid())
    {
    }

    public Guid? TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public DateOnly Date { get; set; }

    public string? Details { get; set; }

    public string? Account { get; set; }

    public string? Category { get; set; }

    public string? Subcategory { get; set; }

    public string? Tags { get; set; }

    public string? Notes { get; set; }

    [Precision(12, 4)]
    public decimal? Debit { get; set; }

    [Precision(12, 4)]
    public decimal? Credit { get; set; }

    [Precision(12, 4)]
    public decimal? Balance { get; set; }

    public string? OriginalDescription { get; set; }

    public DateTime Imported { get; set; }

    public Transaction Transaction { get; set; } = null!;
}
