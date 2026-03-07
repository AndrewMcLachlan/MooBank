using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[PrimaryKey(nameof(Id))]
public class ExchangeRate([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public ExchangeRate() : this(default) { }

    [Required]
    public required string From { get; set; }

    [Required]
    public required string To { get; set; }

    [Precision(12, 4)]
    public decimal Rate { get; set; }

    [Precision(12, 4)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal ReverseRate { get; private set; }

    public DateTime LastUpdated { get; set; }
}
