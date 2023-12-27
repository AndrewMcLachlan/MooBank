namespace Asm.MooBank.Modules.Account.Models.Account;

public partial record VirtualAccount : TransactionAccount
{
    public Guid ParentId { get; set; }
}

public partial record VirtualAccount
{
    public static implicit operator VirtualAccount(Domain.Entities.Account.VirtualAccount account)
    {
        return new VirtualAccount
        {
            Id = account.Id,
            ParentId = account.Id,
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
    public static Domain.Entities.Account.VirtualAccount ToEntity(this VirtualAccount account, Guid parentAccountId) => new(account.Id)
    {
        ParentAccountId = parentAccountId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
    };
}
