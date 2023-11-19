namespace Asm.MooBank.Models.Extensions;

public static class AccountHolderExtensions
{
    public static Domain.Entities.AccountHolder.AccountHolder ToDomain(this AccountHolder accountHolder)
    {
        return new Domain.Entities.AccountHolder.AccountHolder
        {
            AccountHolderId = accountHolder.Id,
            EmailAddress = accountHolder.EmailAddress,
            FirstName = accountHolder.FirstName,
            LastName = accountHolder.LastName,
        };
    }

    public static AccountHolder ToModel(this Domain.Entities.AccountHolder.AccountHolder accountHolder)
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
