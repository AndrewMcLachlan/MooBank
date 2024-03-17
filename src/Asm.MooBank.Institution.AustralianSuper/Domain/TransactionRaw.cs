using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Institution.AustralianSuper.Domain;

[AggregateRoot]
internal class TransactionRaw(Guid id) : KeyedEntity<Guid>(id)
{
    public TransactionRaw() : this(Guid.Empty) { }

    public Guid? TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public DateOnly Date { get; set; }
    public string? Category { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly? PaymentPeriodStart { get; set; }
    public DateOnly? PaymentPeriodEnd { get; set; }
    public decimal? SGContributions { get; set; }
    public decimal? EmployerAdditional { get; set; }
    public decimal? SalarySacrifice { get; set; }
    public decimal? MemberAdditional { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Imported { get; set; }

    public Transaction Transaction { get; set; } = null!;
}
