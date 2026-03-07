using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
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

    [ForeignKey(nameof(AccountHolderId))]
    public User.User? User { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }

    [ForeignKey(nameof(AccountId))]
    public StockHolding.StockHolding StockHolding { get; set; } = null!;
}
