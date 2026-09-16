using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Budget;

[PrimaryKey(nameof(Id))]
public partial class BudgetLine(Guid id) : KeyedEntity<Guid>(id)
{
    public BudgetLine() : this(default) { }

    public int TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    [Navigation]
    public virtual partial Tag.Tag Tag { get; set; }

    [MaxLength(255)]
    public string? Notes { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    public bool Income { get; set; }

    public short Month { get; set; } = 4095; // Bits representing selected months

    public Guid BudgetId { get; set; }

    [ForeignKey(nameof(BudgetId))]
    [Navigation]
    public virtual partial Budget Budget { get; set; }
}
