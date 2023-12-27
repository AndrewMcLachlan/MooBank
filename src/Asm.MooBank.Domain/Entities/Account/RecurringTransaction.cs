using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

public class RecurringTransaction([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public RecurringTransaction() : this(default) { }
    public Guid VirtualAccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime? LastRun { get; set; }

    public virtual VirtualAccount VirtualAccount { get; set; } = null!;

    [Column("ScheduleId")]
    public virtual ScheduleFrequency Schedule { get; set; }
}
