using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

public abstract class TransactionInstrument(Guid id) : Instrument(id)
{
    public DateOnly? LastTransaction { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();

    [Precision(12, 4)]
    public decimal CalculatedBalance { get; set; }

}
