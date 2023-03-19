namespace Asm.MooBank.Models;

public partial record AccountHolder
{
    public static implicit operator Domain.Entities.AccountHolder.AccountHolder(AccountHolder accountHolder)
    {
        return new Domain.Entities.AccountHolder.AccountHolder
        {
            AccountHolderId = accountHolder.Id,
            EmailAddress = accountHolder.EmailAddress,
            FirstName = accountHolder.FirstName,
            LastName = accountHolder.LastName,
        };
    }

    public static implicit operator AccountHolder(Domain.Entities.AccountHolder.AccountHolder accountHolder)
    {
        return new AccountHolder
        {
            Id = accountHolder.AccountHolderId,
            EmailAddress = accountHolder.EmailAddress,
            FirstName = accountHolder.FirstName,
            LastName = accountHolder.LastName,
        };
    }
}
