using Asm.MooBank.Modules.Account.Models.Recurring;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Models.Account;

public record VirtualAccount : TransactionAccount
{
    public Guid ParentId { get; set; }

    public IEnumerable<RecurringTransaction> RecurringTransactions { get; set; } = [];
}

public static class VirtualAccountExtensions
{
    public static VirtualAccount ToModel(this Domain.Entities.Account.VirtualAccount account, ICurrencyConverter currencyConverter)
    {
        return new VirtualAccount
        {
            Id = account.Id,
            ParentId = account.ParentAccountId,
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

    public static IEnumerable<VirtualAccount> ToModel(this IEnumerable<Domain.Entities.Account.VirtualAccount> accounts, ICurrencyConverter currencyConverter) =>
        accounts.Select(a => a.ToModel(currencyConverter));

    public static Domain.Entities.Account.VirtualAccount ToEntity(this VirtualAccount account, Guid parentAccountId) => new(account.Id)
    {
        ParentAccountId = parentAccountId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
    };
}
