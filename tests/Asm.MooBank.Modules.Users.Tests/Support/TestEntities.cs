#nullable enable
using Bogus;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;
using DomainUserCard = Asm.MooBank.Domain.Entities.User.UserCard;
using ModelUserCard = Asm.MooBank.Modules.Users.Models.UserCard;
using ModelUpdateUser = Asm.MooBank.Modules.Users.Models.UpdateUser;

namespace Asm.MooBank.Modules.Users.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainUser CreateDomainUser(
        Guid? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string currency = "AUD",
        Guid? familyId = null,
        Guid? primaryAccountId = null,
        IEnumerable<DomainUserCard>? cards = null)
    {
        var userId = id ?? Guid.NewGuid();
        var user = new DomainUser(userId)
        {
            EmailAddress = email ?? Faker.Internet.Email(),
            FirstName = firstName ?? Faker.Name.FirstName(),
            LastName = lastName ?? Faker.Name.LastName(),
            Currency = currency,
            FamilyId = familyId ?? Guid.NewGuid(),
            PrimaryAccountId = primaryAccountId,
        };

        if (cards != null)
        {
            foreach (var card in cards)
            {
                card.UserId = userId;
                user.Cards.Add(card);
            }
        }

        return user;
    }

    public static DomainUserCard CreateDomainUserCard(
        Guid? userId = null,
        short last4Digits = 1234,
        string? name = null)
    {
        return new DomainUserCard
        {
            UserId = userId ?? Guid.NewGuid(),
            Last4Digits = last4Digits,
            Name = name ?? Faker.Finance.CreditCardNumber()[^4..],
        };
    }

    public static ModelUserCard CreateModelUserCard(
        short last4Digits = 1234,
        string? name = null)
    {
        return new ModelUserCard
        {
            Last4Digits = last4Digits,
            Name = name ?? "My Card",
        };
    }

    public static ModelUpdateUser CreateUpdateUser(
        string currency = "AUD",
        Guid? primaryAccountId = null,
        IEnumerable<ModelUserCard>? cards = null)
    {
        return new ModelUpdateUser
        {
            Currency = currency,
            PrimaryAccountId = primaryAccountId,
            Cards = cards ?? [],
        };
    }

    public static IQueryable<DomainUser> CreateUserQueryable(IEnumerable<DomainUser> users)
    {
        return QueryableHelper.CreateAsyncQueryable(users);
    }

    public static IQueryable<DomainUser> CreateUserQueryable(params DomainUser[] users)
    {
        return CreateUserQueryable(users.AsEnumerable());
    }

    public static List<DomainUserCard> CreateSampleCards(Guid? userId = null)
    {
        var id = userId ?? Guid.NewGuid();
        return
        [
            CreateDomainUserCard(id, 1234, "Personal Card"),
            CreateDomainUserCard(id, 5678, "Work Card"),
        ];
    }
}
