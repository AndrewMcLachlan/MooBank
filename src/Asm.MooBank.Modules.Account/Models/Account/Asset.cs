using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Models.Account;

// Duplicate for summary view.
public record Asset : Instrument
{
}

public static class AssetExtensions
{
    public static Asset ToModel(this Domain.Entities.Asset.Asset account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        Currency = account.Currency,
        CurrentBalance = account.Balance,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
        BalanceDate = account.LastUpdated,
        AccountType = "Asset",
    };

    public static IEnumerable<Asset> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities, ICurrencyConverter currencyConverter) =>
        entities.Select(t => t.ToModel(currencyConverter));
}
