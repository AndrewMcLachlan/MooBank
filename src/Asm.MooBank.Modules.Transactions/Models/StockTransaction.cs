using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Models;
public record StockTransaction
{
    public Guid Id { get; set; } = new Guid();
    public Guid AccountId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }

    public decimal Fees { get; set; }

    public string? AccountHolderName { get; set; }

    public DateTimeOffset TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }
}
