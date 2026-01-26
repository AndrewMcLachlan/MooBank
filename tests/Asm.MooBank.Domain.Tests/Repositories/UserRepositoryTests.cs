using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="UserRepository"/> class.
/// Tests verify user operations against an in-memory database.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public UserRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given a user exists
    /// When Get by id is called
    /// Then the user should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new UserRepository(_context);

        // Act
        var result = await repository.Get(userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test@test.com", result.EmailAddress);
    }

    /// <summary>
    /// Given multiple users exist
    /// When Get all is called
    /// Then all users should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_MultipleUsers_ReturnsAll()
    {
        // Arrange
        var user1 = CreateUser(Guid.NewGuid(), "user1@test.com");
        var user2 = CreateUser(Guid.NewGuid(), "user2@test.com");

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new UserRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetByCard

    /// <summary>
    /// Given a user with a card
    /// When GetByCard is called with matching last 4 digits
    /// Then the user should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByCard_MatchingCard_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.Cards.Add(new UserCard { UserId = userId, Last4Digits = 1234 });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new UserRepository(_context);

        // Act
        var result = await repository.GetByCard(1234, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    /// <summary>
    /// Given a user with a card
    /// When GetByCard is called with non-matching last 4 digits
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByCard_NonMatchingCard_ReturnsNull()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());
        user.Cards.Add(new UserCard { UserId = user.Id, Last4Digits = 1234 });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new UserRepository(_context);

        // Act
        var result = await repository.GetByCard(5678, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    #endregion

    private static User CreateUser(Guid id, string email = "test@test.com") =>
        new(id)
        {
            EmailAddress = email,
            Currency = "AUD",
            FamilyId = Guid.NewGuid(),
        };
}
