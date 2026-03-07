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

    /// <summary>
    /// Given an authenticated principal without currency
    /// When GetCurrentUser is called
    /// Then InvalidOperationException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_MissingCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => provider.GetCurrentUser());
    }

    /// <summary>
    /// Given an authenticated principal with multiple account claims
    /// When GetCurrentUser is called
    /// Then all accounts should be included in the user's Accounts collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_MultipleAccountClaims_ReturnsAllAccounts()
    {
        // Arrange
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        var account3 = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
            new(MooBank.Security.ClaimTypes.AccountId, account1.ToString()),
            new(MooBank.Security.ClaimTypes.AccountId, account2.ToString()),
            new(MooBank.Security.ClaimTypes.AccountId, account3.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(3, result.Accounts.Count());
        Assert.Contains(account1, result.Accounts);
        Assert.Contains(account2, result.Accounts);
        Assert.Contains(account3, result.Accounts);
    }

    /// <summary>
    /// Given an authenticated principal with shared account claims
    /// When GetCurrentUser is called
    /// Then all shared accounts should be included in the user's SharedAccounts collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_SharedAccountClaims_ReturnsSharedAccounts()
    {
        // Arrange
        var sharedAccount1 = Guid.NewGuid();
        var sharedAccount2 = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
            new(MooBank.Security.ClaimTypes.SharedAccountId, sharedAccount1.ToString()),
            new(MooBank.Security.ClaimTypes.SharedAccountId, sharedAccount2.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(2, result.SharedAccounts.Count());
        Assert.Contains(sharedAccount1, result.SharedAccounts);
        Assert.Contains(sharedAccount2, result.SharedAccounts);
    }

    /// <summary>
    /// Given an authenticated principal with group claims
    /// When GetCurrentUser is called
    /// Then all groups should be included in the user's Groups collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_GroupClaims_ReturnsGroups()
    {
        // Arrange
        var group1 = Guid.NewGuid();
        var group2 = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
            new(MooBank.Security.ClaimTypes.GroupId, group1.ToString()),
            new(MooBank.Security.ClaimTypes.GroupId, group2.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(2, result.Groups.Count());
        Assert.Contains(group1, result.Groups);
        Assert.Contains(group2, result.Groups);
    }

    /// <summary>
    /// Given an authenticated principal with primary account claim
    /// When GetCurrentUser is called
    /// Then PrimaryAccountId should be set correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_PrimaryAccountClaim_ReturnsPrimaryAccountId()
    {
        // Arrange
        var primaryAccountId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
            new(MooBank.Security.ClaimTypes.PrimaryAccountId, primaryAccountId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Equal(primaryAccountId, result.PrimaryAccountId);
    }

    /// <summary>
    /// Given an authenticated principal without primary account claim
    /// When GetCurrentUser is called
    /// Then PrimaryAccountId should be null
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_NoPrimaryAccountClaim_ReturnsNullPrimaryAccountId()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Null(result.PrimaryAccountId);
    }

    /// <summary>
    /// Given an authenticated principal with valid UserId claim
    /// When CurrentUserId is accessed
    /// Then the correct UserId should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void CurrentUserId_WithValidClaim_ReturnsUserId()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act & Assert
        Assert.Equal(TestUserId, provider.CurrentUserId);
    }

    /// <summary>
    /// Given an authenticated principal without name claims
    /// When GetCurrentUser is called
    /// Then FirstName and LastName should be null
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_NoNameClaims_ReturnsNullNames()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
    }

    /// <summary>
    /// Given an authenticated principal with no account, shared account, or group claims
    /// When GetCurrentUser is called
    /// Then collections should be empty
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrentUser_NoCollectionClaims_ReturnsEmptyCollections()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(MooBank.Security.ClaimTypes.UserId, TestUserId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "test@test.com"),
            new(MooBank.Security.ClaimTypes.Currency, "AUD"),
            new(MooBank.Security.ClaimTypes.FamilyId, TestFamilyId.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var principalProviderMock = new Mock<IPrincipalProvider>();
        principalProviderMock.Setup(p => p.Principal).Returns(principal);
        var provider = new ClaimsUserDataProvider(principalProviderMock.Object);

        // Act
        var result = provider.GetCurrentUser();

        // Assert
        Assert.Empty(result.Accounts);
        Assert.Empty(result.SharedAccounts);
        Assert.Empty(result.Groups);
    }

    #endregion
}
