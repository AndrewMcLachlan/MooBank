namespace Asm.MooBank.Models.Extensions;

public static class AccountHolderExtensions
{
    public static User ToModel(this Domain.Entities.User.User user)
    {
        return new User
        {
            Id = user.Id,
            EmailAddress = user.EmailAddress,
            FamilyId = user.FamilyId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Currency = user.Currency,
        };
    }
}
