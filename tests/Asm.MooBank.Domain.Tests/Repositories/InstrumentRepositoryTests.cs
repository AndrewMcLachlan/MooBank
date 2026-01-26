using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="InstrumentRepository"/> class.
/// Tests verify instrument CRUD operations against an in-memory database.
/// </summary>
public class InstrumentRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Models.User _user;

    public InstrumentRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _user = CreateTestUser();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get All

    /// <summary>
    /// Given instruments exist for user's accounts
    /// When Get is called
    /// Then only instruments in user's accounts should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithUserAccounts_ReturnsUserInstruments()
    {
        // Arrange
        var userAccountId = Guid.NewGuid();
        var sharedAccountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();

        var userAccount = CreateInstrument(userAccountId, "User Account");
        var sharedAccount = CreateInstrument(sharedAccountId, "Shared Account");
        var otherAccount = CreateInstrument(otherAccountId, "Other Account");

        _context.Set<LogicalAccount>().AddRange(userAccount, sharedAccount, otherAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = new Models.User
        {
            Id = _userId,
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
            Accounts = [userAccountId],
            SharedAccounts = [sharedAccountId],
        };

        var repository = new InstrumentRepository(_context, user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Id == userAccountId);
        Assert.Contains(result, i => i.Id == sharedAccountId);
        Assert.DoesNotContain(result, i => i.Id == otherAccountId);
    }

    /// <summary>
    /// Given user has no accounts
    /// When Get is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_NoUserAccounts_ReturnsEmpty()
    {
        // Arrange
        var user = new Models.User
        {
            Id = _userId,
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
            Accounts = [],
            SharedAccounts = [],
        };

        var repository = new InstrumentRepository(_context, user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Get By Id

    /// <summary>
    /// Given an instrument exists
    /// When Get by id is called
    /// Then the instrument should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingInstrument_ReturnsInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = CreateInstrument(instrumentId, "Test Instrument");
        _context.Set<LogicalAccount>().Add(instrument);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstrumentRepository(_context, _user);

        // Act
        var result = await repository.Get(instrumentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Instrument", result.Name);
    }

    #endregion

    #region Get By Multiple Ids

    /// <summary>
    /// Given multiple instruments exist
    /// When Get by ids is called
    /// Then matching instruments should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByIds_WithMultipleIds_ReturnsMatchingInstruments()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var instrument1 = CreateInstrument(id1, "Instrument 1");
        var instrument2 = CreateInstrument(id2, "Instrument 2");
        var instrument3 = CreateInstrument(id3, "Instrument 3");

        _context.Set<LogicalAccount>().AddRange(instrument1, instrument2, instrument3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstrumentRepository(_context, _user);

        // Act
        var result = await repository.Get([id1, id3], TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Id == id1);
        Assert.Contains(result, i => i.Id == id3);
    }

    /// <summary>
    /// Given empty id list
    /// When Get by ids is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByIds_EmptyIdList_ReturnsEmpty()
    {
        // Arrange
        var repository = new InstrumentRepository(_context, _user);

        // Act
        var result = await repository.Get([], TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new instrument
    /// When Add is called
    /// Then the instrument should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewInstrument_PersistsInstrument()
    {
        // Arrange
        var repository = new InstrumentRepository(_context, _user);
        var instrument = CreateInstrument(Guid.NewGuid(), "New Instrument");

        // Act
        repository.Add(instrument);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedInstrument = await _context.Set<LogicalAccount>().FirstOrDefaultAsync(i => i.Name == "New Instrument", TestContext.Current.CancellationToken);
        Assert.NotNull(savedInstrument);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing instrument
    /// When Update is called
    /// Then the instrument should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingInstrument_UpdatesInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = CreateInstrument(instrumentId, "Original Name");
        _context.Set<LogicalAccount>().Add(instrument);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstrumentRepository(_context, _user);

        // Act
        instrument.Name = "Updated Name";
        repository.Update(instrument);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedInstrument = await _context.Set<LogicalAccount>().FindAsync([instrumentId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedInstrument!.Name);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given an existing instrument
    /// When Delete is called
    /// Then the instrument should have ClosedDate set (soft delete)
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingInstrument_SetsClosedDate()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = CreateInstrument(instrumentId, "To Delete");
        _context.Set<LogicalAccount>().Add(instrument);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstrumentRepository(_context, _user);

        // Act
        repository.Delete(instrumentId);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedInstrument = await _context.Set<LogicalAccount>().FindAsync([instrumentId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(deletedInstrument!.ClosedDate);
    }

    /// <summary>
    /// Given a non-existent instrument
    /// When Delete is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public void Delete_NonExistentInstrument_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new InstrumentRepository(_context, _user);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => repository.Delete(Guid.NewGuid()));
    }

    #endregion

    private static LogicalAccount CreateInstrument(Guid id, string name) =>
        new(id, [])
        {
            Name = name,
            Currency = "AUD",
        };

    private Models.User CreateTestUser() =>
        new()
        {
            Id = _userId,
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
            Accounts = [],
            SharedAccounts = [],
        };
}
