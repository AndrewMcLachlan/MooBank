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

    public IReadOnlyList<ReportKind> AvailableReports { get; init; } = [];

    public IReadOnlyList<TagPurpose> AvailableTagPurposes { get; init; } = [];

    public IReadOnlyList<TagPurposeAssignment> TagPurposes { get; init; } = [];
}

public static class LogicalAccountExtensions
{
    public static async Task<LogicalAccount> ToModel(this Domain.Entities.Account.LogicalAccount account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        List<MooBank.Models.VirtualInstrument> virtualInstruments = [];

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
            AccountType = account.AccountType,
            Currency = account.Currency,
            CurrentBalance = account.Balance,
            CurrentBalanceLocalCurrency = await currencyConverter.Convert(account.Balance, account.Currency, cancellationToken),
            BalanceDate = ((Domain.Entities.Instrument.Instrument)account).LastUpdated,
            LastTransaction = account.LastTransaction,
            InstrumentType = account.AccountType.ToString(),
            Controller = account.Controller,
            ShareWithFamily = account.ShareWithFamily,
            IncludeInBudget = account.IncludeInBudget,
            InstitutionAccounts = account.InstitutionAccounts?.ToModel() ?? [],
            VirtualInstruments = virtualInstruments,
            RemainingBalance = remainingBalance,
            RemainingBalanceLocalCurrency = remainingBalanceLocalCurrency,
            AvailableReports = AccountTypeReports.For(account.AccountType),
            AvailableTagPurposes = AccountTagPurposes.For(account.AccountType),
            TagPurposes = [.. account.TagPurposes.Select(t => new TagPurposeAssignment { Purpose = t.Purpose, TagId = t.TagId })],
        };
    }

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

    public static async Task<LogicalAccount> ToModelWithGroup(this Domain.Entities.Account.LogicalAccount entity, User user, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        var result = await entity.ToModel(currencyConverter, cancellationToken);
        result.GroupId = entity.GetGroup(user.Id)?.Id;

        return result;
    }


    public static async Task<IEnumerable<LogicalAccount>> ToModelAsync(this IQueryable<Domain.Entities.Account.LogicalAccount> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken)
    {
        List<LogicalAccount> models = [];

        foreach (var entity in await entities.ToListAsync(cancellationToken))
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
