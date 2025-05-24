using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[PrimaryKey(nameof(Id))]
public class CpiChange([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public CpiChange() : this(default) { }

    public QuarterEntity Quarter { get; set; } = new(2025, 1);

    [Precision(7, 4)]
    public decimal ChangePercent { get; set; } = default!;
}
