#nullable enable
using Bogus;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;
using ModelGroup = Asm.MooBank.Modules.Groups.Models.Group;

namespace Asm.MooBank.Modules.Groups.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainGroup CreateGroup(
        Guid? id = null,
        string? name = null,
        string? description = null,
        Guid? ownerId = null,
        bool showPosition = false)
    {
        return new DomainGroup(id ?? Guid.NewGuid())
        {
            Name = name ?? Faker.Commerce.Department(),
            Description = description ?? Faker.Lorem.Sentence(),
            OwnerId = ownerId ?? Guid.NewGuid(),
            ShowPosition = showPosition,
        };
    }

    public static ModelGroup CreateGroupModel(
        Guid? id = null,
        string? name = null,
        string? description = null,
        bool showTotal = false)
    {
        return new ModelGroup
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? Faker.Commerce.Department(),
            Description = description ?? Faker.Lorem.Sentence(),
            ShowTotal = showTotal,
        };
    }

    public static List<DomainGroup> CreateSampleGroups(Guid? ownerId = null)
    {
        var owner = ownerId ?? Guid.NewGuid();
        return
        [
            CreateGroup(name: "Savings", ownerId: owner),
            CreateGroup(name: "Investments", ownerId: owner),
            CreateGroup(name: "Everyday", ownerId: owner),
        ];
    }

    public static IQueryable<DomainGroup> CreateGroupQueryable(IEnumerable<DomainGroup> groups)
    {
        return QueryableHelper.CreateAsyncQueryable(groups);
    }

    public static IQueryable<DomainGroup> CreateGroupQueryable(params DomainGroup[] groups)
    {
        return CreateGroupQueryable(groups.AsEnumerable());
    }
}
