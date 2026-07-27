using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Models.Account;

public static class VirtualInstrumentExtensions
{
    public static async Task<VirtualInstrument> ToModel(this Domain.Entities.Instrument.VirtualInstrument virtualInstrument, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        return new VirtualInstrument
        {
            Id = virtualInstrument.Id,
            ParentId = virtualInstrument.ParentInstrumentId,
            Name = virtualInstrument.Name,
            Description = virtualInstrument.Description,
            Controller = virtualInstrument.Controller,
            Currency = virtualInstrument.Currency,
            CurrentBalance = virtualInstrument.Balance,
            CurrentBalanceLocalCurrency = await currencyConverter.Convert(virtualInstrument.Balance, virtualInstrument.Currency, cancellationToken),
            BalanceDate = virtualInstrument.LastUpdated,
            LastTransaction = virtualInstrument.LastTransaction,
            ClosedDate = virtualInstrument.ClosedDate,
        };
    }

    public static async Task<IEnumerable<VirtualInstrument>> ToModel(this IEnumerable<Domain.Entities.Instrument.VirtualInstrument> virtualInstruments, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) =>
        await virtualInstruments.SelectAsync(virtualInstrument => virtualInstrument.ToModel(currencyConverter, cancellationToken));
}
