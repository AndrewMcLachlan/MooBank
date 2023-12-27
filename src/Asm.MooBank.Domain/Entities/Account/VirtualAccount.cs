namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualAccount(Guid id) : TransactionAccount(id)
{
    public Guid ParentAccountId { get; set; }

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
