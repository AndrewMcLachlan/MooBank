using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Stocks.Models;
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

public static class StockTransactionExtensions
{
    public static StockTransaction ToModel(this Domain.Entities.Transactions.StockTransaction stockTransaction) =>
        new()
        {
            AccountHolderName = stockTransaction.User?.FirstName,
            Description = stockTransaction.Description,
            TransactionDate = stockTransaction.TransactionDate,
            AccountId = stockTransaction.AccountId,
            Id = stockTransaction.Id,
            Price = stockTransaction.Price,
            Quantity = stockTransaction.Quantity,
            Fees = stockTransaction.Fees,
            TransactionType = stockTransaction.TransactionType,
        };

    public static IEnumerable<StockTransaction> ToModel(this IEnumerable<Domain.Entities.Transactions.StockTransaction> entities) =>
        entities.Select(t => t.ToModel());

    public static IQueryable<StockTransaction> ToModel(this IQueryable<Domain.Entities.Transactions.StockTransaction> entities) =>
        entities.Select(t => t.ToModel());

}
