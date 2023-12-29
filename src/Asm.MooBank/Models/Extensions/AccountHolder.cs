namespace Asm.MooBank.Models.Extensions;

public static class AccountHolderExtensions
{
    public static AccountHolder ToModel(this Domain.Entities.AccountHolder.AccountHolder accountHolder)
    {
        return new AccountHolder
        {
            Id = accountHolder.Id,
            EmailAddress = accountHolder.EmailAddress,
            FamilyId = accountHolder.FamilyId,
            FirstName = accountHolder.FirstName,
            LastName = accountHolder.LastName,
            Currency = accountHolder.Currency,
        };
    }
}
