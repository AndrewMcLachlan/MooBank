#nullable enable
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Bogus;
using DomainStockHolding = Asm.MooBank.Domain.Entities.StockHolding.StockHolding;
using DomainStockTransaction = Asm.MooBank.Domain.Entities.Transactions.StockTransaction;

namespace Asm.MooBank.Modules.Stocks.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainStockHolding CreateStockHolding(
        Guid? id = null,
        string? name = null,
        string? description = null,
        string? symbol = null,
        decimal currentPrice = 100m,
        int quantity = 10,
        decimal gainLoss = 0m,
        bool shareWithFamily = false,
        Controller controller = Controller.Manual,
        IEnumerable<DomainStockTransaction>? transactions = null)
    {
        var holdingId = id ?? Guid.NewGuid();
        var stockHolding = new DomainStockHolding(holdingId)
        {
            Name = name ?? Faker.Company.CompanyName(),
            Description = description ?? Faker.Lorem.Sentence(),
            Symbol = symbol ?? Faker.Finance.Currency().Code,
            CurrentPrice = currentPrice,
            Quantity = quantity,
            GainLoss = gainLoss,
            ShareWithFamily = shareWithFamily,
            Controller = controller,
        };

        if (transactions != null)
        {
            foreach (var transaction in transactions)
            {
                transaction.AccountId = holdingId;
                stockHolding.Transactions.Add(transaction);
            }
        }

        return stockHolding;
    }

    public static DomainStockTransaction CreateStockTransaction(
        Guid? id = null,
        Guid? accountId = null,
        int quantity = 10,
        decimal price = 100m,
        decimal fees = 10m,
        string? description = null,
        DateTime? transactionDate = null,
        TransactionType transactionType = TransactionType.Credit)
    {
        return new DomainStockTransaction(id ?? Guid.NewGuid())
        {
            AccountId = accountId ?? Guid.NewGuid(),
            Quantity = quantity,
            Price = price,
            Fees = fees,
            Description = description ?? Faker.Commerce.ProductName(),
            TransactionDate = transactionDate ?? DateTime.UtcNow,
            TransactionType = transactionType,
        };
    }

    public static IQueryable<DomainStockHolding> CreateStockHoldingQueryable(IEnumerable<DomainStockHolding> stockHoldings)
    {
        return QueryableHelper.CreateAsyncQueryable(stockHoldings);
    }

    public static IQueryable<DomainStockHolding> CreateStockHoldingQueryable(params DomainStockHolding[] stockHoldings)
    {
        return CreateStockHoldingQueryable(stockHoldings.AsEnumerable());
    }

    public static IQueryable<DomainStockTransaction> CreateStockTransactionQueryable(IEnumerable<DomainStockTransaction> transactions)
    {
        return QueryableHelper.CreateAsyncQueryable(transactions);
    }

    public static IQueryable<DomainStockTransaction> CreateStockTransactionQueryable(params DomainStockTransaction[] transactions)
    {
        return CreateStockTransactionQueryable(transactions.AsEnumerable());
    }

    public static List<DomainStockTransaction> CreateSampleTransactions(Guid? accountId = null)
    {
        var id = accountId ?? Guid.NewGuid();
        return
        [
            CreateStockTransaction(accountId: id, quantity: 10, price: 100m, transactionType: TransactionType.Credit),
            CreateStockTransaction(accountId: id, quantity: 5, price: 110m, transactionType: TransactionType.Credit),
            CreateStockTransaction(accountId: id, quantity: -3, price: 120m, transactionType: TransactionType.Debit),
        ];
    }
}
