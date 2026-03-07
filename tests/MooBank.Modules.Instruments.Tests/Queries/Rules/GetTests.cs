#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.Rules;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingRule_ReturnsRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "WOOLWORTHS");
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable);
        var query = new Get(instrumentId, 1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("WOOLWORTHS", result.Contains);
    }

    [Fact]
    public async Task Handle_MultipleRules_ReturnsCorrectOne()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rules = TestEntities.CreateSampleRules(instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: rules);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable);
        var query = new Get(instrumentId, 2);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Id);
        Assert.Equal("COLES", result.Contains);
    }

    [Fact]
    public async Task Handle_InstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateInstrumentQueryable([]);

        var handler = new GetHandler(queryable);
        var query = new Get(Guid.NewGuid(), 1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_RuleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable);
        var query = new Get(instrumentId, 999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_RuleWithTags_ReturnsTags()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag1 = TestEntities.CreateTag(id: 1, name: "Groceries");
        var tag2 = TestEntities.CreateTag(id: 2, name: "Food");
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "WOOLWORTHS", tags: [tag1, tag2]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable);
        var query = new Get(instrumentId, 1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Tags.Count());
    }

    [Fact]
    public async Task Handle_RuleWithDeletedTags_ExcludesDeletedTags()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var activeTag = TestEntities.CreateTag(id: 1, name: "Active", deleted: false);
        var deletedTag = TestEntities.CreateTag(id: 2, name: "Deleted", deleted: true);
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "WOOLWORTHS", tags: [activeTag, deletedTag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable);
        var query = new Get(instrumentId, 1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("Active", result.Tags.First().Name);
    }
}
