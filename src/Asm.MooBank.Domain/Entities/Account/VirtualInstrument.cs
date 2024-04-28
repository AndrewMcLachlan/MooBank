using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualInstrument(Guid id) : TransactionInstrument(id)
{
    public Guid ParentInstrumentId { get; set; }

    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new HashSet<RecurringTransaction>();


    public void AddRecurringTransaction(string? description, decimal amount, ScheduleFrequency schedule)
    {
        RecurringTransactions.Add(new()
        {
            Amount = amount,
            Description = description,
            VirtualAccountId = Id,
            Schedule = schedule,
        });
    }
}
