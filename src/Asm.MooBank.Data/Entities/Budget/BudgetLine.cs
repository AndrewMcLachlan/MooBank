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

    public short Month { get; set; } = 4095; // Bits representing selected months

    public Guid AccountId { get; set; }

    public virtual Account.Account Account { get; set; }
}
