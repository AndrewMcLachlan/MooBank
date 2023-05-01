using Asm.Domain;
using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
public class BudgetLine : KeyedEntity<Guid>
{
    public BudgetLine(Guid id) : base(id) { }

    public int TagId { get; set; }

    public virtual TransactionTag Tag { get; set; }

    public decimal Amount { get; set; }

    public bool Income { get; set; }

    public int? Month { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account.Account Account { get; set; }
}
