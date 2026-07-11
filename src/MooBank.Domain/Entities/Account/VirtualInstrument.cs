using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Events;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualInstrument : TransactionInstrument
{
    internal VirtualInstrument(Guid id) : base(id) { }

    // For EF materialisation only. Construct through Create.
    internal VirtualInstrument() : this(Guid.Empty) { }

    public static VirtualInstrument Create(string name, string? description, Controller controller, string currency) =>
        new()
        {
            Name = name,
            Description = description,
            Controller = controller,
            Currency = currency,
        };

    public Guid ParentInstrumentId { get; set; }

    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new HashSet<RecurringTransaction>();

    public void AdjustBalance(decimal newBalance, string source)
    {
        var amount = newBalance - Balance;

        Events.Add(new BalanceAdjustmentEvent(this, amount, source));
    }


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

    public void RemoveRecurringTransaction(Guid recurringTransactionId)
    {
        var recurringTransaction = RecurringTransactions.SingleOrDefault(r => r.Id == recurringTransactionId) ?? throw new NotFoundException("Recurring transaction not found");
        RecurringTransactions.Remove(recurringTransaction);
    }
}
