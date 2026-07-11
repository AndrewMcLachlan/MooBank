using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

public abstract class TransactionInstrument : Instrument
{
    protected TransactionInstrument(Guid id) : base(id)
    {
    }

    private decimal? _balance;
    private DateOnly? _lastTransaction;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];

    /// <summary>
    /// Set-based balance derived from the transaction history, surfaced through the auto-included
    /// <see cref="BalanceInfo"/> navigation (the dbo.TransactionInstrumentBalance view).
    /// </summary>
    public InstrumentBalance? BalanceInfo { get; set; }

    /// <summary>
    /// The instrument's current balance. Derived from <see cref="BalanceInfo"/> (the view) at
    /// runtime; the setter exists only so unit tests can construct an instrument with a known
    /// balance without a database. A set value is used only when <see cref="BalanceInfo"/> is not
    /// loaded, so production reads always reflect the view.
    /// </summary>
    [NotMapped]
    public decimal Balance
    {
        get => BalanceInfo?.Balance ?? _balance ?? 0m;
        set => _balance = value;
    }

    /// <inheritdoc cref="Balance"/>
    [NotMapped]
    public DateOnly? LastTransaction
    {
        get => BalanceInfo?.LastTransaction ?? _lastTransaction;
        set => _lastTransaction = value;
    }
}
