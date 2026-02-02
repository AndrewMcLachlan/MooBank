#nullable enable
using Asm.MooBank.Modules.Users.Queries;
using Asm.MooBank.Modules.Users.Tests.Support;

namespace Asm.MooBank.Modules.Users.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsUser()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var domainUser = TestEntities.CreateDomainUser(
            id: userId,
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe");

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test@example.com", result.EmailAddress);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCurrency()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var domainUser = TestEntities.CreateDomainUser(
            id: userId,
            currency: "USD");

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsFamilyId()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var familyId = Guid.NewGuid();
        var domainUser = TestEntities.CreateDomainUser(
            id: userId,
            familyId: familyId);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(familyId, result.FamilyId);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPrimaryAccountId()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var primaryAccountId = Guid.NewGuid();
        var domainUser = TestEntities.CreateDomainUser(
            id: userId,
            primaryAccountId: primaryAccountId);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(primaryAccountId, result.PrimaryAccountId);
    }

    [Fact]
    public async Task Handle_UserWithCards_ReturnsCards()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var cards = new[]
        {
            TestEntities.CreateDomainUserCard(userId, 1234, "Personal Card"),
            TestEntities.CreateDomainUserCard(userId, 5678, "Work Card"),
        };
        var domainUser = TestEntities.CreateDomainUser(
            id: userId,
            cards: cards);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Cards.Count());
        Assert.Contains(result.Cards, c => c.Last4Digits == 1234 && c.Name == "Personal Card");
        Assert.Contains(result.Cards, c => c.Last4Digits == 5678 && c.Name == "Work Card");
    }

    [Fact]
    public async Task Handle_UserWithNoCards_ReturnsEmptyCards()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var domainUser = TestEntities.CreateDomainUser(id: userId);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Cards);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsAccountsFromUserModel()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var accounts = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _mocks.SetUser(TestMocks.CreateTestUser(id: userId, accounts: accounts));

        var domainUser = TestEntities.CreateDomainUser(id: userId);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Accounts.Count());
        Assert.Contains(accounts[0], result.Accounts);
        Assert.Contains(accounts[1], result.Accounts);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsSharedAccountsFromUserModel()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var sharedAccounts = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _mocks.SetUser(TestMocks.CreateTestUser(id: userId, sharedAccounts: sharedAccounts));

        var domainUser = TestEntities.CreateDomainUser(id: userId);

        var users = TestEntities.CreateUserQueryable(domainUser);

        var handler = new GetHandler(users, _mocks.User);

        var query = new Get();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.SharedAccounts.Count());
        Assert.Contains(sharedAccounts[0], result.SharedAccounts);
        Assert.Contains(sharedAccounts[1], result.SharedAccounts);
    }
}
