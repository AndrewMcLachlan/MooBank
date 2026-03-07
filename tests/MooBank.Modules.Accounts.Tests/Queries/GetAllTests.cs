#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Accounts.Queries;
using Asm.MooBank.Modules.Accounts.Tests.Support;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_NoAccounts_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateLogicalAccountQueryable();

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_OwnedAccounts_ReturnsOwnedAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = CreateAccountWithOwner(userId, name: "My Account");
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("My Account", result.First().Name);
    }

    [Fact]
    public async Task Handle_MultipleOwnedAccounts_ReturnsAll()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var accounts = new[]
        {
            CreateAccountWithOwner(userId, name: "Account 1"),
            CreateAccountWithOwner(userId, name: "Account 2"),
            CreateAccountWithOwner(userId, name: "Account 3"),
        };
        var queryable = TestEntities.CreateLogicalAccountQueryable(accounts);

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_ClosedAccounts_ExcludesClosedAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var openAccount = CreateAccountWithOwner(userId, name: "Open Account");
        var closedAccount = CreateAccountWithOwner(userId, name: "Closed Account");
        closedAccount.ClosedDate = DateOnly.FromDateTime(DateTime.Today);

        var queryable = TestEntities.CreateLogicalAccountQueryable(openAccount, closedAccount);

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Open Account", result.First().Name);
    }

    [Fact]
    public async Task Handle_OtherUserAccounts_ExcludesOtherUserAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var otherId = Guid.NewGuid();
        var myAccount = CreateAccountWithOwner(userId, name: "My Account");
        var otherAccount = CreateAccountWithOwner(otherId, name: "Other Account");

        var queryable = TestEntities.CreateLogicalAccountQueryable(myAccount, otherAccount);

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("My Account", result.First().Name);
    }

    [Fact]
    public async Task Handle_PrimaryAccount_SetsPrimaryFlag()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var primaryId = Guid.NewGuid();
        var accounts = new[]
        {
            CreateAccountWithOwner(userId, name: "Regular Account"),
            CreateAccountWithOwner(userId, id: primaryId, name: "Primary Account"),
        };
        var queryable = TestEntities.CreateLogicalAccountQueryable(accounts);

        _mocks.SetUser(TestMocks.CreateTestUser(id: userId, primaryAccountId: primaryId));

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var primary = result.Single(a => a.Id == primaryId);
        Assert.True(primary.IsPrimary);
    }

    [Fact]
    public async Task Handle_NoPrimaryAccountSet_NoneMarkedPrimary()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = CreateAccountWithOwner(userId, name: "Account");
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        // User has no primary account set
        _mocks.SetUser(TestMocks.CreateTestUser(id: userId, primaryAccountId: null));

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result, a => Assert.False(a.IsPrimary));
    }

    [Fact]
    public async Task Handle_FamilySharedAccounts_IncludesFamilySharedAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var familyId = _mocks.User.FamilyId;
        var familyMemberId = Guid.NewGuid();

        var myAccount = CreateAccountWithOwner(userId, name: "My Account");
        var familyAccount = CreateFamilySharedAccount(familyMemberId, familyId, name: "Family Account");

        var queryable = TestEntities.CreateLogicalAccountQueryable(myAccount, familyAccount);

        var handler = new GetAllHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.Name == "My Account");
        Assert.Contains(result, a => a.Name == "Family Account");
    }

    private static LogicalAccount CreateAccountWithOwner(Guid userId, string? name = null, Guid? id = null)
    {
        var accountId = id ?? Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(id: accountId, name: name);
        var owner = new InstrumentOwner
        {
            UserId = userId,
            InstrumentId = accountId,
            User = new DomainUser(userId) { EmailAddress = "test@example.com" },
        };
        account.Owners.Add(owner);
        return account;
    }

    private static LogicalAccount CreateFamilySharedAccount(Guid ownerId, Guid familyId, string? name = null)
    {
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(id: accountId, name: name, shareWithFamily: true);
        var owner = new InstrumentOwner
        {
            UserId = ownerId,
            InstrumentId = accountId,
            User = new DomainUser(ownerId) { EmailAddress = "test@example.com", FamilyId = familyId },
        };
        account.Owners.Add(owner);
        return account;
    }
}
