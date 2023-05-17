namespace Asm.MooBank.Models;

public partial record InstitutionAccount
{
    public static implicit operator InstitutionAccount(Domain.Entities.Account.InstitutionAccount account)
    {
        return new InstitutionAccount
        {
            Id = account.AccountId,
            Name = account.Name,
            Description = account.Description,
            CurrentBalance = account.Balance,
            BalanceDate = ((Domain.Entities.Account.Account)account).LastUpdated,
            AccountType = account.AccountType,
            Controller = account.AccountController,
            ImporterTypeId = account.ImportAccount?.ImporterTypeId,
            VirtualAccounts = account.VirtualAccounts != null && account.VirtualAccounts.Any() ?
                              account.VirtualAccounts.Select(v => (VirtualAccount)v)
                                                     .Union(new[] { new VirtualAccount { Id = Guid.Empty, Name = "Remaining", Balance = account.Balance - account.VirtualAccounts.Sum(v => v.Balance) } }).ToArray() :
                                Array.Empty<VirtualAccount>(),
        };
    }

    public static explicit operator Domain.Entities.Account.InstitutionAccount(InstitutionAccount account)
    {
        return new Domain.Entities.Account.InstitutionAccount
        {
            AccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
            Name = account.Name,
            Description = account.Description,
            Balance = account.CurrentBalance,
            IncludeInPosition = account.IncludeInPosition,
            LastUpdated = account.BalanceDate,
            AccountType = account.AccountType,
            AccountController = account.Controller,
            ImportAccount = account.ImporterTypeId == null ? null : new Domain.Entities.Account.ImportAccount { ImporterTypeId = account.ImporterTypeId.Value, AccountId = account.Id },
        };
    }
}


public static class InstitutionAccountExtensions
{
    public static InstitutionAccount ToModel(this Domain.Entities.Account.InstitutionAccount entity, Guid userId)
    {
        var result = (InstitutionAccount)entity;
        result.AccountGroupId = entity.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static async Task<InstitutionAccount?> ToModelAsync(this Task<Domain.Entities.Account.InstitutionAccount?> entityTask, Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await entityTask.WaitAsync(cancellationToken);

        if (entity == null) return null;

        var result = (InstitutionAccount)entity;
        result.AccountGroupId = entity.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<InstitutionAccount> ToModel(this IEnumerable<Domain.Entities.Account.InstitutionAccount> entities)
    {
        return entities.Select(t => (InstitutionAccount)t);
    }

    public static async Task<IEnumerable<InstitutionAccount>> ToModelAsync(this Task<IEnumerable<Domain.Entities.Account.InstitutionAccount>> entityTask, CancellationToken cancellationToken = default)
    {
        return (await entityTask.WaitAsync(cancellationToken)).Select(t => (InstitutionAccount)t);
    }
}