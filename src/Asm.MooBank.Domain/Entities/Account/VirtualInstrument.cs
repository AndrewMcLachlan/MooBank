namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualInstrument(Guid id) : TransactionAccount(id)
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
