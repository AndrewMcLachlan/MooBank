using System.Security.Claims;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.Security;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Unit tests for the <see cref="SettableUserDataProvider"/> class.
/// Tests cover both settable and claims-based user resolution.
/// </summary>
public class SettableUserDataProviderTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestAccountId = Guid.NewGuid();
    private static readonly Guid TestFamilyId = Guid.NewGuid();

    #region SetUser and GetCurrentUser

    /// <summary>
    /// Given a user has been set
    /// When GetCurrentUser is called
    /// Then the set user should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_AfterSetUser_ReturnsSetUser()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        var provider = new SettableUserDataProvider(principalProviderMock.Object);
        var user = CreateUser();

        // Act
        provider.SetUser(user);
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.EmailAddress, result.EmailAddress);
    }

    /// <summary>
    /// Given a null user
    /// When SetUser is called
    /// Then ArgumentNullException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetUser_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        var provider = new SettableUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.SetUser(null!));
    }

    #endregion

    #region CurrentUserId

    /// <summary>
    /// Given a user has been set
    /// When CurrentUserId is accessed
    /// Then the set user's Id should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CurrentUserId_AfterSetUser_ReturnsSetUserId()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        var provider = new SettableUserDataProvider(principalProviderMock.Object);
        var user = CreateUser();

        // Act
        provider.SetUser(user);

        // Assert
        Assert.Equal(user.Id, provider.CurrentUserId);
    }

    /// <summary>
    /// Given no user has been set and no principal exists
    /// When CurrentUserId is accessed
    /// Then Guid.Empty should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CurrentUserId_NoPrincipal_ReturnsGuidEmpty()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns((ClaimsPrincipal)null!);
        var provider = new SettableUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Equal(Guid.Empty, provider.CurrentUserId);
    }

    #endregion

    #region GetCurrentUser from Claims

    /// <summary>
    /// Given no user has been set and no principal exists
    /// When GetCurrentUser is called
    /// Then InvalidOperationException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_NoPrincipalNoSetUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns((ClaimsPrincipal)null!);
        var provider = new SettableUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => provider.GetCurrentUser());
    }

    /// <summary>
    /// Given no user has been set and principal is not authenticated
    /// When GetCurrentUser is called
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_UnauthenticatedPrincipal_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new SettableUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Null(result);
    }

    #endregion

    private static User CreateUser() =>
        new()
        {
            Id = TestUserId,
            EmailAddress = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            Currency = "AUD",
            FamilyId = TestFamilyId,
            Accounts = [TestAccountId],
        };
}

/// <summary>
/// Unit tests for the <see cref="ClaimsUserDataProvider"/> class.
/// Tests cover claims-based user resolution.
/// </summary>
public class ClaimsUserDataProviderTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestFamilyId = Guid.NewGuid();
    private static readonly Guid TestAccountId = Guid.NewGuid();

    #region CurrentUserId

    /// <summary>
    /// Given no principal exists
    /// When CurrentUserId is accessed
    /// Then Guid.Empty should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CurrentUserId_NoPrincipal_ReturnsGuidEmpty()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns((ClaimsPrincipal)null!);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Equal(Guid.Empty, provider.CurrentUserId);
    }

    #endregion

    #region GetCurrentUser

    /// <summary>
    /// Given no principal exists
    /// When GetCurrentUser is called
    /// Then InvalidOperationException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_NoPrincipal_ThrowsInvalidOperationException()
    {
        // Arrange
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns((ClaimsPrincipal)null!);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => provider.GetCurrentUser());
    }

    /// <summary>
    /// Given principal is not authenticated
    /// When GetCurrentUser is called
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_UnauthenticatedPrincipal_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Given an authenticated principal with required claims
    /// When GetCurrentUser is called
    /// Then a User with correct properties should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_AuthenticatedPrincipal_ReturnsUser()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(System.Security.Claims.ClaimTypes.GivenName, "Test"),
            new(System.Security.Claims.ClaimTypes.Surname, "User"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.AccountId, TestAccountId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(TestUserId, result.Id);
        Assert.Equal("test@test.com", result.EmailAddress);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("AUD", result.Currency);
        Assert.Contains(TestAccountId, result.Accounts);
    }

    /// <summary>
    /// Given an authenticated principal without email
    /// When GetCurrentUser is called
    /// Then InvalidOperationException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_MissingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => provider.GetCurrentUser());
    }

    #endregion
}
