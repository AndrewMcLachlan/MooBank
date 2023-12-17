using Asm.MooBank.Modules.Transactions.Models;

namespace Asm.MooBank.Modules.Transactions;
public static class StockTransactionExtensions
{
    public static StockTransaction ToModel(this Domain.Entities.Transactions.StockTransaction stockTransaction) =>
        new()
        {
            AccountHolderName = stockTransaction.AccountHolder?.FirstName,
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
