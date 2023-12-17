using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
public class Budget(Guid id) : KeyedEntity<Guid>(id)
{
    public short Year { get; set; }

    public Guid FamilyId { get; set; }

    public virtual Family.Family Family { get; set; } = null!;

    public virtual ICollection<BudgetLine> Lines { get; set; } = new HashSet<BudgetLine>();
}
