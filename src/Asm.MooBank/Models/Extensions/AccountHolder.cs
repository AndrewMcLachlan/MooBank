namespace Asm.MooBank.Models.Extensions;

public static class AccountHolderExtensions
{
    public static User ToModel(this Domain.Entities.AccountHolder.User accountHolder)
    {
        return new User
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
