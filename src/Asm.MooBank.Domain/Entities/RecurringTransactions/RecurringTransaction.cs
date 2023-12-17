using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Domain.Entities.RecurringTransactions;

public class RecurringTransaction
{
    public int RecurringTransactionId { get; set; }
    public Guid VirtualAccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime? LastRun { get; set; }

    public virtual VirtualAccount VirtualAccount { get; set; } = null!;

    [Column("ScheduleId")]
    public virtual ScheduleFrequency Schedule { get; set; }
}
