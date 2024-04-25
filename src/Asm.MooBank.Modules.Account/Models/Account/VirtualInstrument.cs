using Asm.MooBank.Modules.Account.Models.Recurring;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Models.Account;

public record VirtualInstrument : TransactionAccount
{
    public Guid ParentId { get; set; }

    public IEnumerable<RecurringTransaction> RecurringTransactions { get; set; } = [];
}

public static class VirtualAccountExtensions
{
    public static VirtualInstrument ToModel(this Domain.Entities.Account.VirtualInstrument account, ICurrencyConverter currencyConverter)
    {
        return new VirtualInstrument
        {
            Id = account.Id,
            ParentId = account.ParentInstrumentId,
            Name = account.Name,
            Description = account.Description,
            Currency = account.Currency,
            CurrentBalance = account.Balance,
            CalculatedBalance = account.CalculatedBalance,
            CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
            RecurringTransactions = account.RecurringTransactions.ToModel(),
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
