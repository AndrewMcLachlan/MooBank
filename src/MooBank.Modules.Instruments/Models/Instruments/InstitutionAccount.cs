using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public static class InstitutionAccountExtensions
{
    public static async Task<InstrumentSummary> ToModel(this Domain.Entities.Account.LogicalAccount account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        List<VirtualInstrument> virtualInstruments = [];

        if (account.VirtualInstruments != null)
        {
            foreach (var virtualInstrument in account.VirtualInstruments.Where(v => v.ClosedDate == null).OrderBy(v => v.Name))
            {
                virtualInstruments.Add(await virtualInstrument.ToModel(currencyConverter, cancellationToken));
            }
        }

        var (remainingBalance, remainingBalanceLocalCurrency) = await Remaining(account, currencyConverter, cancellationToken);

        return new()
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            InstrumentType = account.AccountType.ToString(),
            Controller = account.Controller,
            Currency = account.Currency,
            CurrentBalance = account.Balance,
            CurrentBalanceLocalCurrency = await currencyConverter.Convert(account.Balance, account.Currency, cancellationToken),
            BalanceDate = ((Domain.Entities.Instrument.Instrument)account).LastUpdated,
            VirtualInstruments = virtualInstruments,
            RemainingBalance = remainingBalance,
            RemainingBalanceLocalCurrency = remainingBalanceLocalCurrency,
        };
    }

    public static async Task<IEnumerable<InstrumentSummary>> ToModel(this IEnumerable<Domain.Entities.Account.LogicalAccount> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        List<InstrumentSummary> models = [];

        foreach (var entity in entities)
        {
            models.Add(await entity.ToModel(currencyConverter, cancellationToken));
        }

        return models;
    }

    private static async Task<(decimal? RemainingBalance, decimal? RemainingBalanceLocalCurrency)> Remaining(Domain.Entities.Account.LogicalAccount account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken)
    {
        if (account.VirtualInstruments == null || account.VirtualInstruments.Count == 0)
        {
            return (null, null);
        }

        var openVirtualInstruments = account.VirtualInstruments.Where(v => v.ClosedDate == null);

        if (!openVirtualInstruments.Any())
        {
            return (null, null);
        }

        var remainingBalance = account.Balance - openVirtualInstruments.Sum(v => v.Balance);

        var remainingBalanceLocalCurrency = await currencyConverter.Convert(remainingBalance, account.Currency, cancellationToken);

        return (remainingBalance, remainingBalanceLocalCurrency);
    }
}
