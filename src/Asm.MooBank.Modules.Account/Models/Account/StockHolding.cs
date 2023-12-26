namespace Asm.MooBank.Modules.Account.Models.Account;

// Duplicate for summary view.
public record StockHolding : Account
{
}

public static class StockHoldingExtensions
{
    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        CurrentBalance = account.CurrentValue,
        BalanceDate = ((Domain.Entities.Account.Account)account).LastUpdated,
        AccountType = "Stock Holding",
    };

    public static IEnumerable<StockHolding> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities) =>
        entities.Select(t => t.ToModel());
}
