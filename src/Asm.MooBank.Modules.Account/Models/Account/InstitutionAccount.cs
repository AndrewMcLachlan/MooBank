using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Models.Account;

public partial record InstitutionAccount : TransactionAccount
{
    private AccountType _accountType;

    public bool IncludeInPosition { get; set; }

    public new AccountType AccountType
    {
        get => _accountType;
        set
        {
            _accountType = value;
            base.AccountType = value.ToString();
        }
    }

    public AccountController Controller { get; set; }

    public int? ImporterTypeId { get; set; }

    public bool IsPrimary { get; set; }

    public bool ShareWithFamily { get; set; }

    public int InstitutionId { get; set; }

    public bool IncludeInBudget { get; init; }

    public decimal VirtualAccountRemainingBalance
    {
        get => CurrentBalance - (VirtualAccounts?.Sum(v => v.CurrentBalance) ?? 0);
    }
}

public static class InstitutionAccountExtensions
{
    public static InstitutionAccount ToModel(this Domain.Entities.Account.InstitutionAccount account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        Currency = account.Currency,
        CurrentBalance = account.Balance,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.Balance, account.Currency),
        CalculatedBalance = account.CalculatedBalance,
        BalanceDate = ((Domain.Entities.Account.Instrument)account).LastUpdated,
        LastTransaction = account.LastTransaction,
        AccountType = account.AccountType,
        Controller = account.AccountController,
        ImporterTypeId = account.ImportAccount?.ImporterTypeId,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionId = account.InstitutionId,
        VirtualAccounts = account.VirtualInstruments != null && account.VirtualInstruments.Count != 0 ?
                          account.VirtualInstruments.OrderBy(v => v.Name).Select(v => v.ToModel(currencyConverter))
                                                 .Union(Remaining(account, currencyConverter)).ToArray() : [],
    };

    public static Domain.Entities.Account.InstitutionAccount ToEntity(this InstitutionAccount account) => new(account.Id == Guid.Empty ? Guid.NewGuid() : account.Id)
    {
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
        IncludeInPosition = account.IncludeInPosition,
        LastUpdated = account.BalanceDate,
        AccountType = account.AccountType,
        AccountController = account.Controller,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionId = account.InstitutionId,
        ImportAccount = account.ImporterTypeId == null ? null : new Domain.Entities.Account.ImportAccount { ImporterTypeId = account.ImporterTypeId.Value, AccountId = account.Id },
    };

    public static InstitutionAccount ToModelWithAccountGroup(this Domain.Entities.Account.InstitutionAccount entity, AccountHolder accountHolder, ICurrencyConverter currencyConverter)
    {
        var result = entity.ToModel(currencyConverter);
        result.AccountGroupId = entity.GetAccountGroup(accountHolder.Id)?.Id;

        return result;
    }

    public static IEnumerable<InstitutionAccount> ToModel(this IEnumerable<Domain.Entities.Account.InstitutionAccount> entities, ICurrencyConverter currencyConverter)
    {
        return entities.Select(t => t.ToModel(currencyConverter));
    }

    public static IQueryable<InstitutionAccount> ToModel(this IQueryable<Domain.Entities.Account.InstitutionAccount> entities, ICurrencyConverter currencyConverter)
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
            Currency = account.Currency,
            CurrentBalance = remainingBalance,
            CurrentBalanceLocalCurrency = currencyConverter.Convert(remainingBalance, account.Currency)
        };
    }
}
