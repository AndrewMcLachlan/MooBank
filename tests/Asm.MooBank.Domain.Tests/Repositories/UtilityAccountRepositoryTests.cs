using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="UtilityAccountRepository"/> class.
/// Tests verify utility account CRUD operations and user authorization.
/// </summary>
public class UtilityAccountRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();

    public UtilityAccountRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get All

    /// <summary>
    /// Given utility accounts exist for user
    /// When Get is called
    /// Then only user's accounts should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithUserAccounts_ReturnsUserAccounts()
    {
        // Arrange
        var userAccountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();

        var userAccount = CreateUtilityAccount(userAccountId, "User Electricity", "ACC001");
        _context.Set<Account>().Add(userAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var otherAccount = CreateUtilityAccount(otherAccountId, "Other Gas", "ACC002");
        _context.Set<Account>().Add(otherAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([userAccountId]); // Only owns userAccountId
        var repository = new UtilityAccountRepository(_context, user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("User Electricity", result.First().Name);
    }

    /// <summary>
    /// Given user has no utility accounts
    /// When Get is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_NoUserAccounts_ReturnsEmpty()
    {
        // Arrange
        var otherAccount = CreateUtilityAccount(Guid.NewGuid(), "Other Account", "ACC001");
        _context.Set<Account>().Add(otherAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([]); // No accounts
        var repository = new UtilityAccountRepository(_context, user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Get By Id

    /// <summary>
    /// Given a utility account exists and user owns it
    /// When Get by id is called
    /// Then the account should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_UserOwnsAccount_ReturnsAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateUtilityAccount(accountId, "Test Account", "ACC001");
        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([accountId]);
        var repository = new UtilityAccountRepository(_context, user);

        // Act
        var result = await repository.Get(accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Account", result.Name);
    }

    /// <summary>
    /// Given a utility account exists but user doesn't own it
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_UserDoesNotOwnAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateUtilityAccount(accountId, "Other Account", "ACC001");
        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([]); // Doesn't own this account
        var repository = new UtilityAccountRepository(_context, user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            repository.Get(accountId, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given a utility account does not exist
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var user = CreateUser([]);
        var repository = new UtilityAccountRepository(_context, user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            repository.Get(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new utility account
    /// When Add is called
    /// Then the account should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewAccount_PersistsAccount()
    {
        // Arrange
        var user = CreateUser([]);
        var repository = new UtilityAccountRepository(_context, user);
        var account = CreateUtilityAccount(Guid.NewGuid(), "New Account", "NEW001");

        // Act
        repository.Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedAccount = await _context.Set<Account>()
            .FirstOrDefaultAsync(a => a.Name == "New Account", TestContext.Current.CancellationToken);
        Assert.NotNull(savedAccount);
        Assert.Equal("NEW001", savedAccount.AccountNumber);
    }

    /// <summary>
    /// Given a utility account with utility type
    /// When Add is called
    /// Then utility type should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_WithUtilityType_PersistsUtilityType()
    {
        // Arrange
        var user = CreateUser([]);
        var repository = new UtilityAccountRepository(_context, user);
        var account = CreateUtilityAccount(Guid.NewGuid(), "Water Account", "WAT001");
        account.UtilityType = UtilityType.Water;

        // Act
        repository.Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedAccount = await _context.Set<Account>()
            .FirstOrDefaultAsync(a => a.Name == "Water Account", TestContext.Current.CancellationToken);
        Assert.Equal(UtilityType.Water, savedAccount!.UtilityType);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing utility account
    /// When Update is called
    /// Then the account should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingAccount_UpdatesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateUtilityAccount(accountId, "Original Name", "ACC001");
        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([accountId]);
        var repository = new UtilityAccountRepository(_context, user);

        // Act
        account.Name = "Updated Name";
        repository.Update(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedAccount = await _context.Set<Account>()
            .FindAsync([accountId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedAccount!.Name);
    }

    /// <summary>
    /// Given a utility account
    /// When account number is updated
    /// Then the account number should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_AccountNumber_PersistsAccountNumber()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = CreateUtilityAccount(accountId, "Test", "OLD001");
        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user = CreateUser([accountId]);
        var repository = new UtilityAccountRepository(_context, user);

        // Act
        account.AccountNumber = "NEW999";
        repository.Update(account);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedAccount = await _context.Set<Account>()
            .FindAsync([accountId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("NEW999", updatedAccount!.AccountNumber);
    }

    #endregion

    #region Helpers

    private Models.User CreateUser(Guid[] accountIds) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = _familyId,
            Currency = "AUD",
            Accounts = accountIds,
            SharedAccounts = [],
        };

    private Account CreateUtilityAccount(Guid id, string name, string accountNumber)
    {
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User.User(userId)
        {
            EmailAddress = $"user-{userId}@test.com",
            FamilyId = _familyId,
        };
        _context.Set<Domain.Entities.User.User>().Add(user);

        return new Account(id)
        {
            Name = name,
            AccountNumber = accountNumber,
            Currency = "AUD",
            UtilityType = UtilityType.Electricity,
            Owners = [new InstrumentOwner { UserId = userId, User = user }],
        };
    }

    #endregion
}
