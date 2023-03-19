namespace Asm.MooBank.Domain.Entities.RecurringTransactions;

public class RecurringTransaction
{
    public int RecurringTransactionId { get; set; }
    public Guid VirtualAccountId { get; set; }
    public string? Description { get; set; }
    public int ScheduleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? LastRun { get; set; }

    public virtual Account.VirtualAccount VirtualAccount { get; set; }
    public virtual ScheduleFrequency Schedule { get; set; }
}
