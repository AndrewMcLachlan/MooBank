using System.ComponentModel.DataAnnotations;
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Budget;

[AggregateRoot]
public class BudgetLine(Guid id) : KeyedEntity<Guid>(id)
{
    public int TagId { get; set; }

    public virtual Tag.Tag Tag { get; set; } = null!;

    [MaxLength(255)]
    public string? Notes { get; set; }

    public decimal Amount { get; set; }

    public bool Income { get; set; }

    public short Month { get; set; } = 4095; // Bits representing selected months

    public Guid BudgetId { get; set; }

    public virtual Budget Budget { get; set; } = null!;
}
