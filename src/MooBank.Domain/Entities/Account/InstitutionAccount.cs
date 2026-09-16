using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(Id))]
public partial class InstitutionAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public InstitutionAccount() : this(Guid.Empty) { }

    public Guid InstrumentId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = String.Empty;

    public int InstitutionId { get; set; }

    public DateOnly OpenedDate { get; set; }

    public DateOnly? ClosedDate { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    [Navigation]
    public virtual partial Institution.Institution Institution { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [Navigation]
    public virtual partial LogicalAccount LogicalAccount { get; set; }
}
