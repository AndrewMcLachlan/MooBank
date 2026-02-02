#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.Rules;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithRules_ReturnsAllRules()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rules = TestEntities.CreateSampleRules(instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: rules);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_NoRules_ReturnsEmptyList()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_InstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateInstrumentQueryable([]);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_RulesWithTags_IncludesTags()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "WOOLWORTHS", tags: [tag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);
        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetAllHandler(queryable);
        var query = new GetAll(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var ruleResult = result.First();
        Assert.Single(ruleResult.Tags);
        Assert.Equal("Groceries", ruleResult.Tags.First().Name);
    }
}
