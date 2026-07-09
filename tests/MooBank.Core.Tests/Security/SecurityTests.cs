#nullable enable
using System.Security.Claims;
using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Unit tests for the <see cref="Asm.MooBank.Security.Security"/> wrapper, which evaluates
/// requirements via IAuthorizationService, audits denials and throws.
/// </summary>
[Trait("Category", "Unit")]
public class SecurityTests
{
    private readonly Mock<IAuthorizationService> _authorizationService = new();
    private readonly Mock<IPrincipalProvider> _principalProvider = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity("TestAuth"));
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = Guid.NewGuid(),
        Currency = "AUD",
    };

    public SecurityTests()
    {
        _principalProvider.Setup(p => p.Principal).Returns(_principal);
    }

    private Asm.MooBank.Security.Security CreateSecurity() =>
        new(_authorizationService.Object, _principalProvider.Object, _user, _audit.Object);

    private void SetupResourceAuthorization(bool succeeds) =>
        _authorizationService
            .Setup(a => a.AuthorizeAsync(_principal, It.IsAny<object?>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed());

    private void SetupPolicyAuthorization(bool succeeds) =>
        _authorizationService
            .Setup(a => a.AuthorizeAsync(_principal, It.IsAny<object?>(), It.IsAny<string>()))
            .ReturnsAsync(succeeds ? AuthorizationResult.Success() : AuthorizationResult.Failed());

    #region AssertGroupPermission

    /// <summary>
    /// Given the group owner requirement succeeds
    /// When AssertGroupPermission is called
    /// Then no exception is thrown and nothing is audited
    /// </summary>
    [Fact]
    public async Task AssertGroupPermission_Authorised_DoesNotThrow()
    {
        // Arrange
        SetupResourceAuthorization(true);

        // Act & Assert
        await CreateSecurity().AssertGroupPermission(Guid.NewGuid());
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given the group owner requirement fails
    /// When AssertGroupPermission is called
    /// Then NotAuthorisedException is thrown and the denial audited
    /// </summary>
    [Fact]
    public async Task AssertGroupPermission_NotAuthorised_ThrowsAndAudits()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        SetupResourceAuthorization(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => CreateSecurity().AssertGroupPermission(groupId));
        _audit.Verify(a => a.AuthorizationDenied(_user, "Group", groupId, nameof(GroupOwnerRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a group entity
    /// When AssertGroupPermission is called with the entity
    /// Then the requirement is evaluated for the group's id
    /// </summary>
    [Fact]
    public async Task AssertGroupPermission_Entity_DelegatesToId()
    {
        // Arrange
        var group = new DomainGroup(Guid.NewGuid()) { Name = "Test Group", OwnerId = _user.Id };
        SetupResourceAuthorization(true);

        // Act
        await CreateSecurity().AssertGroupPermission(group);

        // Assert
        _authorizationService.Verify(
            a => a.AuthorizeAsync(_principal, (object?)group.Id, It.IsAny<IEnumerable<IAuthorizationRequirement>>()),
            Times.Once);
    }

    #endregion

    #region AssertFamilyPermission

    /// <summary>
    /// Given the family member requirement succeeds
    /// When AssertFamilyPermission is called
    /// Then no exception is thrown
    /// </summary>
    [Fact]
    public async Task AssertFamilyPermission_Authorised_DoesNotThrow()
    {
        // Arrange
        SetupResourceAuthorization(true);

        // Act & Assert
        await CreateSecurity().AssertFamilyPermission(_user.FamilyId);
    }

    /// <summary>
    /// Given the family member requirement fails
    /// When AssertFamilyPermission is called
    /// Then NotAuthorisedException is thrown and the denial audited
    /// </summary>
    [Fact]
    public async Task AssertFamilyPermission_NotAuthorised_ThrowsAndAudits()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        SetupResourceAuthorization(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => CreateSecurity().AssertFamilyPermission(familyId));
        _audit.Verify(a => a.AuthorizationDenied(_user, "Family", familyId, nameof(FamilyMemberRequirement)), Times.Once);
    }

    #endregion

    #region AssertAdministrator

    /// <summary>
    /// Given the Admin policy succeeds
    /// When AssertAdministrator is called
    /// Then no exception is thrown
    /// </summary>
    [Fact]
    public async Task AssertAdministrator_Authorised_DoesNotThrow()
    {
        // Arrange
        SetupPolicyAuthorization(true);

        // Act & Assert
        await CreateSecurity().AssertAdministrator(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Given the Admin policy fails
    /// When AssertAdministrator is called
    /// Then NotAuthorisedException is thrown and the denial audited
    /// </summary>
    [Fact]
    public async Task AssertAdministrator_NotAuthorised_ThrowsAndAudits()
    {
        // Arrange
        SetupPolicyAuthorization(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => CreateSecurity().AssertAdministrator(TestContext.Current.CancellationToken));
        _audit.Verify(a => a.AuthorizationDenied(_user, "Administrator", null, Policies.Admin), Times.Once);
    }

    #endregion
}
