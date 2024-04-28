using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;
public class ExchangeRate([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public ExchangeRate() : this(default) { }

    public string From { get; set; } = null!;

    public string To { get; set; } = null!;

    [Precision(12, 4)]
    public decimal Rate { get; set; }

    [Precision(12, 4)]
    public decimal ReverseRate { get; private set; }

    public DateTime LastUpdated { get; set; }
}
