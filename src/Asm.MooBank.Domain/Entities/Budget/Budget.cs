using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class Budget(Guid id) : KeyedEntity<Guid>(id)
{
    public Budget() : this(Guid.Empty) { }

    public short Year { get; set; }

    public Guid FamilyId { get; set; }

    [ForeignKey(nameof(FamilyId))]
    public virtual Family.Family Family { get; set; } = null!;

    public virtual ICollection<BudgetLine> Lines { get; set; } = new HashSet<BudgetLine>();
}
