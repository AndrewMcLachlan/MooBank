using Asm.MooBank.Models;

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
    public static InstitutionAccount ToModel(this Domain.Entities.Account.InstitutionAccount account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        CurrentBalance = account.Balance,
        CalculatedBalance = account.CalculatedBalance,
        BalanceDate = ((Domain.Entities.Account.Account)account).LastUpdated,
        LastTransaction = account.LastTransaction,
        AccountType = account.AccountType,
        Controller = account.AccountController,
        ImporterTypeId = account.ImportAccount?.ImporterTypeId,
        ShareWithFamily = account.ShareWithFamily,
        IncludeInBudget = account.IncludeInBudget,
        InstitutionId = account.InstitutionId,
        VirtualAccounts = account.VirtualAccounts != null && account.VirtualAccounts.Count != 0 ?
                          account.VirtualAccounts.OrderBy(v => v.Name).Select(v => v.ToModel())
                                                 .Union(new[] { new VirtualAccount { Id = Guid.Empty, Name = "Remaining", CurrentBalance = account.Balance - account.VirtualAccounts.Sum(v => v.Balance) } }).ToArray() : [],
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

    public static InstitutionAccount ToModel(this Domain.Entities.Account.InstitutionAccount entity, Guid userId)
    {
        var result = entity.ToModel();
        result.AccountGroupId = entity.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static async Task<InstitutionAccount?> ToModelAsync(this Task<Domain.Entities.Account.InstitutionAccount?> entityTask, Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await entityTask.WaitAsync(cancellationToken);

        if (entity == null) return null;

        var result = entity.ToModel();
        result.AccountGroupId = entity.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<InstitutionAccount> ToModel(this IEnumerable<Domain.Entities.Account.InstitutionAccount> entities)
    {
        return entities.Select(t => t.ToModel());
    }

    public static async Task<IEnumerable<InstitutionAccount>> ToModelAsync(this Task<List<Domain.Entities.Account.InstitutionAccount>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => t.ToModel());
    }
}