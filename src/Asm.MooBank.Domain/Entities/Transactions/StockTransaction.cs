using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
public class StockTransaction : Entity, IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Fees { get; set; }

    public Guid? AccountHolderId { get; internal set; }
    public AccountHolder.AccountHolder? AccountHolder { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }

    public StockHolding.StockHolding StockHolding { get; set; } = null!;
}
