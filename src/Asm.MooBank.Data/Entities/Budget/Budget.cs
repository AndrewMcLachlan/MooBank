using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
public class Budget : KeyedEntity<Guid>
{
    public Budget(Guid id) : base(id)
    {
        Lines = new HashSet<BudgetLine>();
    }

    public short Year { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account.Account Account { get; set; }

    public virtual ICollection<BudgetLine> Lines { get; set; }

}
