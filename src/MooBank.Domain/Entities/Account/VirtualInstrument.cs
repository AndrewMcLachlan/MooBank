using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualInstrument(Guid id) : TransactionInstrument(id)
{
    public VirtualInstrument() : this(Guid.Empty) { }

    public Guid ParentInstrumentId { get; set; }

    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new HashSet<RecurringTransaction>();


    public RecurringTransaction AddRecurringTransaction(string? description, decimal amount, ScheduleFrequency schedule, DateOnly nextRun)
    {
        var recurringTransaction = new RecurringTransaction
        {
            Amount = amount,
            Description = description,
            VirtualAccountId = Id,
            Schedule = schedule,
            NextRun = nextRun
        };

        RecurringTransactions.Add(recurringTransaction);

        return recurringTransaction;
    }
}
