using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="RuleRepository"/> class.
/// Tests verify rule CRUD operations against an in-memory database.
/// </summary>
public class RuleRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _instrumentId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();

    public RuleRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        SetupInstrument();
    }

    private void SetupInstrument()
    {
        var instrument = new LogicalAccount(_instrumentId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
        };
        _context.Set<LogicalAccount>().Add(instrument);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetForInstrument

    /// <summary>
    /// Given rules exist for an instrument
    /// When GetForInstrument is called
    /// Then all rules for that instrument should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetForInstrument_WithExistingRules_ReturnsRulesForInstrument()
    {
        // Arrange
        var rule1 = CreateRule(1, "AMAZON", _instrumentId);
        var rule2 = CreateRule(2, "NETFLIX", _instrumentId);
        var otherInstrumentRule = CreateRule(3, "OTHER", Guid.NewGuid());

        _context.Set<Rule>().AddRange(rule1, rule2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RuleRepository(_context);

        // Act
        var result = await repository.GetForInstrument(_instrumentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal(_instrumentId, r.InstrumentId));
    }

    /// <summary>
    /// Given rules exist
    /// When GetForInstrument is called
    /// Then rules should be ordered by Contains
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetForInstrument_ReturnsOrderedByContains()
    {
        // Arrange
        var ruleZ = CreateRule(1, "ZZZZ", _instrumentId);
        var ruleA = CreateRule(2, "AAAA", _instrumentId);
        var ruleM = CreateRule(3, "MMMM", _instrumentId);

        _context.Set<Rule>().AddRange(ruleZ, ruleA, ruleM);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RuleRepository(_context);

        // Act
        var result = (await repository.GetForInstrument(_instrumentId, TestContext.Current.CancellationToken)).ToList();

        // Assert
        Assert.Equal("AAAA", result[0].Contains);
        Assert.Equal("MMMM", result[1].Contains);
        Assert.Equal("ZZZZ", result[2].Contains);
    }

    /// <summary>
    /// Given no rules exist for an instrument
    /// When GetForInstrument is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetForInstrument_NoRules_ReturnsEmpty()
    {
        // Arrange
        var repository = new RuleRepository(_context);

        // Act
        var result = await repository.GetForInstrument(_instrumentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Get by Id

    /// <summary>
    /// Given a rule exists
    /// When Get by id is called
    /// Then the rule should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingRule_ReturnsRule()
    {
        // Arrange
        var rule = CreateRule(1, "AMAZON", _instrumentId);
        _context.Set<Rule>().Add(rule);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RuleRepository(_context);

        // Act
        var result = await repository.Get(_instrumentId, 1, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AMAZON", result.Contains);
    }

    /// <summary>
    /// Given a rule does not exist
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_NonExistentRule_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new RuleRepository(_context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(_instrumentId, 999, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given a rule belongs to a different instrument
    /// When Get by id is called with wrong instrument
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_WrongInstrument_ThrowsNotFoundException()
    {
        // Arrange
        var rule = CreateRule(1, "AMAZON", _instrumentId);
        _context.Set<Rule>().Add(rule);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RuleRepository(_context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(Guid.NewGuid(), 1, TestContext.Current.CancellationToken));
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new rule
    /// When Add is called
    /// Then the rule should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewRule_PersistsRule()
    {
        // Arrange
        var repository = new RuleRepository(_context);
        var rule = CreateRule(0, "NEW_RULE", _instrumentId);

        // Act
        repository.Add(rule);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedRule = await _context.Set<Rule>().FirstOrDefaultAsync(r => r.Contains == "NEW_RULE", TestContext.Current.CancellationToken);
        Assert.NotNull(savedRule);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given an existing rule
    /// When Delete is called
    /// Then the rule should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingRule_RemovesRule()
    {
        // Arrange
        var rule = CreateRule(1, "TO_DELETE", _instrumentId);
        _context.Set<Rule>().Add(rule);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RuleRepository(_context);

        // Act
        await repository.Delete(_instrumentId, 1, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedRule = await _context.Set<Rule>().FirstOrDefaultAsync(r => r.Id == 1, TestContext.Current.CancellationToken);
        Assert.Null(deletedRule);
    }

    /// <summary>
    /// Given a non-existent rule
    /// When Delete is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_NonExistentRule_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new RuleRepository(_context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Delete(_instrumentId, 999, TestContext.Current.CancellationToken));
    }

    #endregion

    private static Rule CreateRule(int id, string contains, Guid instrumentId) =>
        new(id)
        {
            Contains = contains,
            InstrumentId = instrumentId,
            Description = $"Rule for {contains}",
        };
}
