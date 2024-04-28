using Asm.MooBank.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Models.Account;

public partial record InstitutionAccount : TransactionInstrument
{
    public AccountType AccountType { get; set; }

    public int? ImporterTypeId { get; set; }

    public bool IsPrimary { get; set; }

    public bool ShareWithFamily { get; set; }

    public int InstitutionId { get; set; }

    public bool IncludeInBudget { get; init; }
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
        BalanceDate = ((Domain.Entities.Instrument.Instrument)account).LastUpdated,
        LastTransaction = account.LastTransaction,
        InstrumentType = account.AccountType.ToString(),
        Controller = account.Controller,
        ImporterTypeId = account.ImportAccount?.ImporterTypeId,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionId = account.InstitutionId,
        VirtualInstruments = account.VirtualInstruments != null && account.VirtualInstruments.Count != 0 ?
                          account.VirtualInstruments.OrderBy(v => v.Name).Select(v => v.ToModel(currencyConverter))
                                                 .Union(Remaining(account, currencyConverter)).ToArray() : [],
    };

    public static Domain.Entities.Account.InstitutionAccount ToEntity(this InstitutionAccount account) => new(account.Id == Guid.Empty ? Guid.NewGuid() : account.Id)
    {
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
        LastUpdated = account.BalanceDate,
        AccountType = account.AccountType,
        Controller = account.Controller,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionId = account.InstitutionId,
        ImportAccount = account.ImporterTypeId == null ? null : new Domain.Entities.Account.ImportAccount { ImporterTypeId = account.ImporterTypeId.Value, AccountId = account.Id },
    };

    public static InstitutionAccount ToModelWithGroup(this Domain.Entities.Account.InstitutionAccount entity, User user, ICurrencyConverter currencyConverter)
    {
        var result = entity.ToModel(currencyConverter);
        result.GroupId = entity.GetGroup(user.Id)?.Id;

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
            Controller = Controller.Virtual,
            Currency = account.Currency,
            CurrentBalance = remainingBalance,
            CurrentBalanceLocalCurrency = currencyConverter.Convert(remainingBalance, account.Currency)
        };
    }
}
