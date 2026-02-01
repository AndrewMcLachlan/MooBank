#nullable enable
using Asm.Drawing;
using Asm.MooBank.Domain.Entities.Tag;
using Bogus;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using ModelTag = Asm.MooBank.Models.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainTag CreateTag(
        int id = 1,
        string? name = null,
        Guid? familyId = null,
        HexColour? colour = null,
        bool deleted = false,
        bool applySmoothing = false,
        bool excludeFromReporting = false,
        IEnumerable<DomainTag>? subTags = null)
    {
        var tag = new DomainTag(id)
        {
            Name = name ?? Faker.Commerce.Department(),
            FamilyId = familyId ?? Guid.NewGuid(),
            Colour = colour,
            Deleted = deleted,
            Settings = new TagSettings
            {
                ApplySmoothing = applySmoothing,
                ExcludeFromReporting = excludeFromReporting,
            },
        };

        if (subTags != null)
        {
            foreach (var subTag in subTags)
            {
                tag.Tags.Add(subTag);
            }
        }

        return tag;
    }

    public static ModelTag CreateTagModel(
        int id = 0,
        string? name = null,
        HexColour? colour = null,
        bool applySmoothing = false,
        bool excludeFromReporting = false,
        IEnumerable<ModelTag>? tags = null)
    {
        return new ModelTag
        {
            Id = id,
            Name = name ?? Faker.Commerce.Department(),
            Colour = colour,
            Tags = tags ?? [],
            Settings = new ModelTag.TagSettings
            {
                ApplySmoothing = applySmoothing,
                ExcludeFromReporting = excludeFromReporting,
            },
        };
    }

    public static Models.UpdateTag CreateUpdateTag(
        string? name = null,
        HexColour? colour = null,
        bool excludeFromReporting = false,
        bool applySmoothing = false)
    {
        return new Models.UpdateTag(
            name ?? Faker.Commerce.Department(),
            colour,
            excludeFromReporting,
            applySmoothing);
    }

    public static List<DomainTag> CreateSampleTags(Guid? familyId = null)
    {
        var fid = familyId ?? Guid.NewGuid();
        return
        [
            CreateTag(id: 1, name: "Groceries", familyId: fid),
            CreateTag(id: 2, name: "Fuel", familyId: fid),
            CreateTag(id: 3, name: "Entertainment", familyId: fid),
            CreateTag(id: 4, name: "Utilities", familyId: fid),
        ];
    }

    public static IQueryable<DomainTag> CreateTagQueryable(IEnumerable<DomainTag> tags)
    {
        return QueryableHelper.CreateAsyncQueryable(tags);
    }

    public static IQueryable<DomainTag> CreateTagQueryable(params DomainTag[] tags)
    {
        return CreateTagQueryable(tags.AsEnumerable());
    }
}
