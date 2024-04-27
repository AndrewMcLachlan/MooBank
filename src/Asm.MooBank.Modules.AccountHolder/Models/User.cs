namespace Asm.MooBank.Modules.Users.Models;

public record User : MooBank.Models.User
{
    public IEnumerable<UserCard> Cards { get; set; } = [];
}

public static class UserExtensions
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
            PrimaryAccountId = user.PrimaryAccountId,
            Cards = user.Cards.Select(c => new UserCard
            {
                Name = c.Name,
                Last4Digits = c.Last4Digits,
            }).ToList(),
        };
    }
}
