#nullable enable
using Asm.MooBank.Modules.Instruments.Models.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Models;

[Trait("Category", "Unit")]
public class RuleExtensionsTests
{
    [Fact]
    public void Rule_ToModel_MapsBasicProperties()
    {
        // Arrange
        var rule = TestEntities.CreateRule(
            id: 42,
            contains: "WOOLWORTHS",
            description: "Grocery shopping");

        // Act
        var model = rule.ToModel();

        // Assert
        Assert.Equal(42, model.Id);
        Assert.Equal("WOOLWORTHS", model.Contains);
        Assert.Equal("Grocery shopping", model.Description);
    }

    [Fact]
    public void Rule_ToModel_WithTags_MapsTags()
    {
        // Arrange
        var tag1 = TestEntities.CreateTag(id: 1, name: "Groceries");
        var tag2 = TestEntities.CreateTag(id: 2, name: "Food");
        var rule = TestEntities.CreateRule(id: 1, contains: "TEST", tags: [tag1, tag2]);

        // Act
        var model = rule.ToModel();

        // Assert
        Assert.Equal(2, model.Tags.Count());
        Assert.Contains(model.Tags, t => t.Name == "Groceries");
        Assert.Contains(model.Tags, t => t.Name == "Food");
    }

    [Fact]
    public void Rule_ToModel_ExcludesDeletedTags()
    {
        // Arrange
        var activeTag = TestEntities.CreateTag(id: 1, name: "Active", deleted: false);
        var deletedTag = TestEntities.CreateTag(id: 2, name: "Deleted", deleted: true);
        var rule = TestEntities.CreateRule(id: 1, contains: "TEST", tags: [activeTag, deletedTag]);

        // Act
        var model = rule.ToModel();

        // Assert
        Assert.Single(model.Tags);
        Assert.Equal("Active", model.Tags.First().Name);
    }

    [Fact]
    public void Rule_ToModel_NoTags_ReturnsEmptyTags()
    {
        // Arrange
        var rule = TestEntities.CreateRule(id: 1, contains: "TEST");

        // Act
        var model = rule.ToModel();

        // Assert
        Assert.Empty(model.Tags);
    }

    [Fact]
    public void Rule_ToModel_NullDescription_ReturnsNull()
    {
        // Arrange
        var rule = new Domain.Entities.Instrument.Rule(1)
        {
            InstrumentId = Guid.NewGuid(),
            Contains = "TEST",
            Description = null,
        };

        // Act
        var model = rule.ToModel();

        // Assert
        Assert.Null(model.Description);
    }

    [Fact]
    public void RuleCollection_ToModel_MapsAllRules()
    {
        // Arrange
        var rules = new[]
        {
            TestEntities.CreateRule(id: 1, contains: "RULE1"),
            TestEntities.CreateRule(id: 2, contains: "RULE2"),
            TestEntities.CreateRule(id: 3, contains: "RULE3"),
        };

        // Act
        var models = rules.ToModel();

        // Assert
        Assert.Equal(3, models.Count());
    }

    [Fact]
    public void RuleCollection_ToModel_PreservesOrder()
    {
        // Arrange
        var rules = new[]
        {
            TestEntities.CreateRule(id: 1, contains: "FIRST"),
            TestEntities.CreateRule(id: 2, contains: "SECOND"),
            TestEntities.CreateRule(id: 3, contains: "THIRD"),
        };

        // Act
        var models = rules.ToModel().ToList();

        // Assert
        Assert.Equal("FIRST", models[0].Contains);
        Assert.Equal("SECOND", models[1].Contains);
        Assert.Equal("THIRD", models[2].Contains);
    }

    [Fact]
    public async Task RuleCollection_ToModelAsync_MapsAllRules()
    {
        // Arrange
        var rules = new[]
        {
            TestEntities.CreateRule(id: 1, contains: "RULE1"),
            TestEntities.CreateRule(id: 2, contains: "RULE2"),
        };
        var task = Task.FromResult<IEnumerable<Domain.Entities.Instrument.Rule>>(rules);

        // Act
        var models = await task.ToModelAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, models.Count());
    }

    [Fact]
    public async Task RuleCollection_ToModelAsync_WithCancellation_Completes()
    {
        // Arrange
        var rules = new[]
        {
            TestEntities.CreateRule(id: 1, contains: "RULE1"),
        };
        var task = Task.FromResult<IEnumerable<Domain.Entities.Instrument.Rule>>(rules);

        // Act
        var models = await task.ToModelAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(models);
    }
}
