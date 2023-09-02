namespace Asm.MooBank.Models;

public partial record VirtualAccount
{
    public static implicit operator VirtualAccount(Domain.Entities.Account.VirtualAccount account)
    {
        return new VirtualAccount
        {
            Id = account.AccountId,
            Name = account.Name,
            Description = account.Description,
            CurrentBalance = account.Balance,
            CalculatedBalance = account.CalculatedBalance,
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
        };
    }
}

public static class VirtualAccountExtensions
{
    public static Domain.Entities.Account.VirtualAccount ToEntity(this VirtualAccount account, Guid parentAccountId)
    {
        return new Domain.Entities.Account.VirtualAccount
        {
            AccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
            InstitutionAccountId = parentAccountId,
            Name = account.Name,
            Description = account.Description,
            Balance = account.CurrentBalance,
        };
    }
}
