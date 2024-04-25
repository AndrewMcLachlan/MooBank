using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.Account;

public abstract class TransactionAccount(Guid id) : Instrument(id)
{
    public DateOnly? LastTransaction { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();

    public decimal CalculatedBalance { get; set; }

}
