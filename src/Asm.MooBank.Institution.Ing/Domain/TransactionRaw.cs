using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.Ing.Domain;

[AggregateRoot]
internal class TransactionRaw : KeyedEntity<Guid>
{
    public TransactionRaw() : this(Guid.NewGuid())
    {
    }

    public TransactionRaw([DisallowNull] Guid id) : base(id)
    {
    }

    public Guid? TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public DateOnly Date { get; set; }
    public string? Description { get; set; } = null!;

    [Precision(12, 4)]
    public decimal? Credit { get; set; }

    [Precision(12, 4)]
    public decimal? Debit { get; set; }

    [Precision(12, 4)]
    public decimal? Balance { get; set; }
    public DateTime Imported { get; set; }

    public Transaction Transaction { get; set; } = null!;
}
