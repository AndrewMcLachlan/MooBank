using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(Id))]
public class InstitutionAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public InstitutionAccount() : this(Guid.Empty) { }

    public Guid InstrumentId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = String.Empty;

    public int InstitutionId { get; set; }

    public int? ImporterTypeId { get; set; }

    public DateOnly OpenedDate { get; set; }

    public DateOnly? ClosedDate { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    [AllowNull]
    public virtual Institution.Institution Institution { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [AllowNull]
    public virtual LogicalAccount LogicalAccount { get; set; }

    [ForeignKey(nameof(ImporterTypeId))]
    [AllowNull]
    public ImporterType ImporterType { get; set; }
}
