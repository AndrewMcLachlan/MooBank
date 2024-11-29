using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public static class VirtualInstrumentExtensions
{
    public static VirtualInstrument ToModel(this Domain.Entities.Account.VirtualInstrument account, ICurrencyConverter currencyConverter)
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
            CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
        };
    }

    public static IEnumerable<VirtualInstrument> ToModel(this IEnumerable<Domain.Entities.Account.VirtualInstrument> accounts, ICurrencyConverter currencyConverter) =>
        accounts.Select(a => a.ToModel(currencyConverter));

    public static Domain.Entities.Account.VirtualInstrument ToEntity(this VirtualInstrument account, Guid parentInstrumentId) => new(account.Id)
    {
        ParentInstrumentId = parentInstrumentId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
    };
}
