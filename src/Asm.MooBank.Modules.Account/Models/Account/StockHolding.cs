using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Models.Account;

// Duplicate for summary view.
public record StockHolding : Account
{
}

public static class StockHoldingExtensions
{
    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        Currency = account.Currency,
        CurrentBalance = account.CurrentValue,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.CurrentValue, account.Currency),
        BalanceDate = ((Domain.Entities.Account.Account)account).LastUpdated,
        AccountType = "Stock Holding",
    };

    public static IEnumerable<StockHolding> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities, ICurrencyConverter currencyConverter) =>
        entities.Select(t => t.ToModel(currencyConverter));
}
