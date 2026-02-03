#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Queries;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(id: accountId, name: "Test Account");
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.Id);
        Assert.Equal("Test Account", result.Name);
    }

    [Fact]
    public async Task Handle_ExistingAccount_MapsBalanceCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(id: accountId, balance: 2500.50m);
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2500.50m, result.CurrentBalance);
    }

    [Fact]
    public async Task Handle_ExistingAccount_ConvertsCurrency()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(id: accountId, currency: "USD", balance: 100m);
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(100m, result.CurrentBalance);
        Assert.Equal(150m, result.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateLogicalAccountQueryable();

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleAccounts_ReturnsCorrectOne()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var accounts = new[]
        {
            TestEntities.CreateLogicalAccount(name: "Account 1"),
            TestEntities.CreateLogicalAccount(id: targetId, name: "Target Account"),
            TestEntities.CreateLogicalAccount(name: "Account 3"),
        };
        var queryable = TestEntities.CreateLogicalAccountQueryable(accounts);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Target Account", result.Name);
    }

    [Fact]
    public async Task Handle_AccountWithInstitutionAccounts_MapsInstitutionAccounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var instAccount = TestEntities.CreateInstitutionAccount(instrumentId: accountId, name: "Inst Account");
        var account = TestEntities.CreateLogicalAccount(id: accountId, institutionAccounts: [instAccount]);
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.InstitutionAccounts);
        Assert.Equal("Inst Account", result.InstitutionAccounts.First().Name);
    }

    [Fact]
    public async Task Handle_AccountWithOwners_IncludesOwnerInfo()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var account = TestEntities.CreateLogicalAccount(
            id: accountId,
            owners: [TestEntities.CreateInstrumentOwner(userId: ownerId)]);
        var queryable = TestEntities.CreateLogicalAccountQueryable(account);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(accountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
    }
}
