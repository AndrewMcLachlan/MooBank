using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Models.Account;

public record VirtualAccount : VirtualInstrument
{
    public IEnumerable<RecurringTransaction> RecurringTransactions { get; set; } = [];
}

public static class VirtualAccountExtensions
{
    public static async Task<VirtualAccount> ToModel(this Domain.Entities.Account.VirtualInstrument account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        return new VirtualAccount
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
            RecurringTransactions = account.RecurringTransactions.ToModel(),
        };
    }

    public static async Task<IEnumerable<VirtualAccount>> ToModel(this IEnumerable<Domain.Entities.Account.VirtualInstrument> accounts, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) =>
        await accounts.SelectAsync(account => account.ToModel(currencyConverter, cancellationToken));

    public static Domain.Entities.Account.VirtualInstrument ToEntity(this VirtualAccount account, Guid parentInstrumentId) => new(account.Id)
    {
        ParentInstrumentId = parentInstrumentId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
        ClosedDate = account.ClosedDate,
    };
}
