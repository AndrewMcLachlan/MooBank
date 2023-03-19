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
            Balance = account.Balance,
        };
    }

    public static explicit operator Domain.Entities.Account.VirtualAccount(VirtualAccount account)
    {
        return new Domain.Entities.Account.VirtualAccount
        {
            AccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
            Name = account.Name,
            Description = account.Description,
            Balance = account.Balance,
        };
    }
}
