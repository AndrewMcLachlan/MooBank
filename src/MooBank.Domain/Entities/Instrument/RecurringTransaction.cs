using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[PrimaryKey(nameof(Id))]
public partial class RecurringTransaction([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public RecurringTransaction() : this(default) { }
    public Guid VirtualInstrumentId { get; set; }
    public string? Description { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }
    public DateTime? LastRun { get; set; }

    public DateOnly NextRun { get; set; } = DateTime.UtcNow.ToDateOnly();

    [Navigation]
    public virtual partial VirtualInstrument VirtualInstrument { get; set; }

    [Column("ScheduleId")]
    public virtual ScheduleFrequency Schedule { get; set; }
}
