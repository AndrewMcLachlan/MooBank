#nullable enable
using Bogus;
using DomainFamily = Asm.MooBank.Domain.Entities.Family.Family;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Families.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainFamily CreateFamily(
        Guid? id = null,
        string? name = null,
        IEnumerable<DomainUser>? members = null)
    {
        var familyId = id ?? Guid.NewGuid();
        var family = new DomainFamily(familyId)
        {
            Name = name ?? Faker.Company.CompanyName() + "'s Family",
        };

        if (members != null)
        {
            foreach (var member in members)
            {
                family.AccountHolders.Add(member);
                member.FamilyId = familyId;
            }
        }

        return family;
    }

    public static DomainUser CreateDomainUser(
        Guid? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        Guid? familyId = null)
    {
        return new DomainUser(id ?? Guid.NewGuid())
        {
            EmailAddress = email ?? Faker.Internet.Email(),
            FirstName = firstName ?? Faker.Name.FirstName(),
            LastName = lastName ?? Faker.Name.LastName(),
            FamilyId = familyId ?? Guid.NewGuid(),
        };
    }

    public static List<DomainFamily> CreateSampleFamilies()
    {
        var family1 = CreateFamily(name: "Smith Family");
        var family2 = CreateFamily(name: "Johnson Family");
        var family3 = CreateFamily(name: "Williams Family");

        return [family1, family2, family3];
    }

    public static IQueryable<DomainFamily> CreateFamilyQueryable(IEnumerable<DomainFamily> families)
    {
        return QueryableHelper.CreateAsyncQueryable(families);
    }

    public static IQueryable<DomainFamily> CreateFamilyQueryable(params DomainFamily[] families)
    {
        return CreateFamilyQueryable(families.AsEnumerable());
    }
}
