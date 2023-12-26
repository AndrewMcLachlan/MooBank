namespace Asm.MooBank.Modules.Stock.Models;
public record StockHolding : Account
{
    public required string Symbol { get; init; }

    public int Quantity { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal CurrentValue { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class StockHoldingExtensions
{
    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Symbol = account.Symbol,
        Description = account.Description,
        CurrentBalance = account.CurrentValue,
        BalanceDate = ((Domain.Entities.Account.Account)account).LastUpdated,
        AccountType = "Stock Holding",
        CurrentPrice = account.CurrentPrice,
        Quantity = account.Quantity,
        CurrentValue = account.CurrentValue,
    };

    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account, Guid userId)
    {
        var result = account.ToModel();
        result.AccountGroupId = account.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<StockHolding> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities)
    {
        return entities.Select(t => t.ToModel());
    }

    public static Domain.Entities.StockHolding.StockHolding ToEntity(this StockHolding account) => new(account.Id == Guid.Empty ? Guid.NewGuid() : account.Id)
    {
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
        LastUpdated = account.BalanceDate,
        ShareWithFamily = account.ShareWithFamily,
        CurrentPrice = account.CurrentPrice,
        Quantity = account.Quantity,

    };

}
