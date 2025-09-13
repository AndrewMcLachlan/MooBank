using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.AustralianSuper.Domain;

[AggregateRoot]
internal class TransactionRaw(Guid id) : KeyedEntity<Guid>(id)
{
    public TransactionRaw() : this(Guid.Empty) { }

    public Guid? TransactionId { get; set; }

    public Guid AccountId { get; set; }

    public DateOnly Date { get; set; }

    public string? Category { get; set; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? PaymentPeriodStart { get; set; }

    public DateOnly? PaymentPeriodEnd { get; set; }

    [Precision(12, 4)]
    public decimal? SGContributions { get; set; }

    [Precision(12, 4)]
    public decimal? EmployerAdditional { get; set; }

    [Precision(12, 4)]
    public decimal? SalarySacrifice { get; set; }

    [Precision(12, 4)]
    public decimal? MemberAdditional { get; set; }

    [Precision(12, 4)]
    public decimal TotalAmount { get; set; }

    public DateTime Imported { get; set; }

    [AllowNull]
    public Transaction Transaction { get; set; }
}
