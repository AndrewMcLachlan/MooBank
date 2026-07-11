using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

public abstract class TransactionInstrument : Instrument
{
    protected TransactionInstrument(Guid id) : base(id)
    {
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateOnly? LastTransaction { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = [];

    [Precision(12, 4)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal Balance { get; set; }

}
