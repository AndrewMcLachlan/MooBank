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

    public DateOnly OpenedDate { get; set; }

    public DateOnly? ClosedDate { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    public virtual Institution.Institution Institution { get; set; } = null!;

    [ForeignKey(nameof(InstrumentId))]
    public virtual LogicalAccount LogicalAccount { get; set; } = null!;
}
