using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class BudgetLine(Guid id) : KeyedEntity<Guid>(id)
{
    public BudgetLine() : this(default) { }

    public int TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public virtual Tag.Tag Tag { get; set; } = null!;

    [MaxLength(255)]
    public string? Notes { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    public bool Income { get; set; }

    public short Month { get; set; } = 4095; // Bits representing selected months

    public Guid BudgetId { get; set; }

    [ForeignKey(nameof(BudgetId))]
    public virtual Budget Budget { get; set; } = null!;
}
