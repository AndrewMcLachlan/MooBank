using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Entities.User.Specifications;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="GetWithCards"/> specification.
/// Tests verify that the specification correctly includes Cards.
/// </summary>
public class GetWithCardsSpecificationTests
{
    #region Basic Application

    /// <summary>
    /// Given a collection of users
    /// When GetWithCards is applied
    /// Then all users should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithUsers_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<DomainUser>
        {
            CreateUser("user1@test.com"),
            CreateUser("user2@test.com"),
            CreateUser("user3@test.com"),
        };

        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(users.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When GetWithCards is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var users = new List<DomainUser>();
        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(users.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single user
    /// When GetWithCards is applied
    /// Then the user should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleUser_ReturnsSingleUser()
    {
        // Arrange
        var users = new List<DomainUser>
        {
            CreateUser("single@test.com"),
        };

        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(users.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("single@test.com", result[0].EmailAddress);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given users with various email addresses
    /// When GetWithCards is applied
    /// Then email addresses should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesUserEmails()
    {
        // Arrange
        var users = new List<DomainUser>
        {
            CreateUser("first@test.com"),
            CreateUser("second@test.com"),
        };

        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(users.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, u => u.EmailAddress == "first@test.com");
        Assert.Contains(result, u => u.EmailAddress == "second@test.com");
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When GetWithCards is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var users = new List<DomainUser>
        {
            CreateUser("keep@test.com"),
            CreateUser("remove@test.com"),
            CreateUser("keep@test.com"),
        };

        var filteredQuery = users.AsQueryable().Where(u => u.EmailAddress == "keep@test.com");
        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.Equal("keep@test.com", u.EmailAddress));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of users
    /// When GetWithCards is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var users = new List<DomainUser>
        {
            CreateUser("test@test.com"),
        };

        var spec = new GetWithCards();

        // Act
        var result = spec.Apply(users.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<DomainUser>>(result);
    }

    #endregion

    private DomainUser CreateUser(string email)
    {
        return new DomainUser(Guid.NewGuid())
        {
            EmailAddress = email,
            FamilyId = TestModels.FamilyId,
        };
    }
}
