using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

public class RecurringTransaction([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public RecurringTransaction() : this(default) { }
    public Guid VirtualAccountId { get; set; }
    public string? Description { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }
    public DateTime? LastRun { get; set; }

    public DateOnly NextRun { get; set; } = DateTime.UtcNow.ToDateOnly();

    public virtual VirtualInstrument VirtualAccount { get; set; } = null!;

    [Column("ScheduleId")]
    public virtual ScheduleFrequency Schedule { get; set; }
}
