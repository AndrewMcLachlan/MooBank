﻿using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Account;

public static class InstitutionAccountExtensions
{
    public static InstrumentSummary ToModel(this Domain.Entities.Account.InstitutionAccount account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        InstrumentType = account.AccountType.ToString(),
        Controller = account.Controller,
        Currency = account.Currency,
        CurrentBalance = account.Balance,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
        BalanceDate = ((Domain.Entities.Instrument.Instrument)account).LastUpdated,
        VirtualInstruments = account.VirtualInstruments != null && account.VirtualInstruments.Count != 0 ?
                          account.VirtualInstruments.OrderBy(v => v.Name).Select(v => v.ToModel(currencyConverter))
                                                 .Union(Remaining(account, currencyConverter)).ToArray() : [],
    };

    public static IEnumerable<InstrumentSummary> ToModel(this IEnumerable<Domain.Entities.Account.InstitutionAccount> entities, ICurrencyConverter currencyConverter)
    {
        return entities.Select(t => t.ToModel(currencyConverter));
    }

    private static IEnumerable<VirtualInstrument> Remaining(Domain.Entities.Account.InstitutionAccount account, ICurrencyConverter currencyConverter)
    {
        var remainingBalance = account.Balance - account.VirtualInstruments.Sum(v => v.Balance);

        yield return new VirtualInstrument
        {
            Id = Guid.Empty,
            Name = "Remaining",
            Controller = account.Controller,
            Currency = account.Currency,
            CurrentBalance = remainingBalance,
            CurrentBalanceLocalCurrency = currencyConverter.Convert(remainingBalance, account.Currency)
        };
    }
}