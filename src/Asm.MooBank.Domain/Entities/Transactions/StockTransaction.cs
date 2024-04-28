using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
public class StockTransaction(Guid id) : KeyedEntity<Guid>(id)
{
    public StockTransaction() : this(default) { }

    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }

    [Precision(12, 4)]
    public decimal Price { get; set; }

    [Precision(12, 4)]
    public decimal Fees { get; set; }

    public Guid? AccountHolderId { get; internal set; }
    public User.User? User { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }

    public StockHolding.StockHolding StockHolding { get; set; } = null!;
}
