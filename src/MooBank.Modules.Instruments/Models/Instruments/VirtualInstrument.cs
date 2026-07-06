using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public static class VirtualInstrumentExtensions
{
    public static async Task<VirtualInstrument> ToModel(this Domain.Entities.Account.VirtualInstrument account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        return new VirtualInstrument
        {
            Id = account.Id,
            ParentId = account.ParentInstrumentId,
            Name = account.Name,
            Description = account.Description,
            Controller = account.Controller,
            Currency = account.Currency,
            CurrentBalance = account.Balance,
            CurrentBalanceLocalCurrency = await currencyConverter.Convert(account.Balance, account.Currency, cancellationToken),
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
            ClosedDate = account.ClosedDate,
        };
    }

    public static async Task<IEnumerable<VirtualInstrument>> ToModel(this IEnumerable<Domain.Entities.Account.VirtualInstrument> accounts, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        List<VirtualInstrument> models = [];

        foreach (var account in accounts)
        {
            models.Add(await account.ToModel(currencyConverter, cancellationToken));
        }

        return models;
    }

    public static Domain.Entities.Account.VirtualInstrument ToEntity(this VirtualInstrument account, Guid parentInstrumentId) => new(account.Id)
    {
        ParentInstrumentId = parentInstrumentId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
        ClosedDate = account.ClosedDate,
    };
}
