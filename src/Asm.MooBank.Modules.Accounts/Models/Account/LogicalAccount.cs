using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Models.Account;

public partial record LogicalAccount : TransactionInstrument
{
    public AccountType AccountType { get; set; }

    public bool IsPrimary { get; set; }

    public bool ShareWithFamily { get; set; }

    public bool IncludeInBudget { get; init; }

    public IEnumerable<InstitutionAccount> InstitutionAccounts { get; init; } = [];
}

public static class LogicalAccountExtensions
{
    public static LogicalAccount ToModel(this Domain.Entities.Account.LogicalAccount account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        AccountType = account.AccountType,
        Currency = account.Currency,
        CurrentBalance = account.Balance,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
        BalanceDate = ((Domain.Entities.Instrument.Instrument)account).LastUpdated,
        LastTransaction = account.LastTransaction,
        InstrumentType = account.AccountType.ToString(),
        Controller = account.Controller,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionAccounts = account.InstitutionAccounts?.ToModel() ?? [],
        VirtualInstruments = account.VirtualInstruments != null && account.VirtualInstruments.Count != 0 ?
                             account.VirtualInstruments.OrderBy(v => v.Name).Select(v => v.ToModel(currencyConverter)).ToArray() : [],
        RemainingBalance = Remaining(account, currencyConverter).RemainingBalance,
        RemainingBalanceLocalCurrency = Remaining(account, currencyConverter).RemainingBalanceLocalCurrency,
    };

    public static Domain.Entities.Account.LogicalAccount ToEntity(this LogicalAccount account) => new(account.Id == Guid.Empty ? Guid.NewGuid() : account.Id, account.InstitutionAccounts.ToEntity())
    {
        Name = account.Name,
        Description = account.Description,
        LastUpdated = account.BalanceDate,
        AccountType = account.AccountType,
        Controller = account.Controller,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
    };

    public static LogicalAccount ToModelWithGroup(this Domain.Entities.Account.LogicalAccount entity, User user, ICurrencyConverter currencyConverter)
    {
        var result = entity.ToModel(currencyConverter);
        result.GroupId = entity.GetGroup(user.Id)?.Id;

        return result;
    }


    public static async Task<IEnumerable<LogicalAccount>> ToModelAsync(this IQueryable<Domain.Entities.Account.LogicalAccount> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken) =>
        (await entities.ToListAsync(cancellationToken)).Select(t => t.ToModel(currencyConverter));


    private static (decimal? RemainingBalance, decimal? RemainingBalanceLocalCurrency) Remaining(Domain.Entities.Account.LogicalAccount account, ICurrencyConverter currencyConverter)
    {
        if (account.VirtualInstruments == null || account.VirtualInstruments.Count == 0)
        {
            return (null, null);
        }

        var remainingBalance = account.Balance - account.VirtualInstruments.Sum(v => v.Balance);

        var remainingBalanceLocalCurrency = currencyConverter.Convert(remainingBalance, account.Currency);

        return (remainingBalance, remainingBalanceLocalCurrency);
    }
}
