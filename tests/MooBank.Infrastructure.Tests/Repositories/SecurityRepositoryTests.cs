#nullable enable
using System.Security.Claims;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

[Trait("Category", "Unit")]
public class SecurityRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IPrincipalProvider> _principalProviderMock;
    private readonly Mock<IAuditLogger> _auditLoggerMock;
    private readonly Models.User _user;
    private readonly ClaimsPrincipal _principal;

    public SecurityRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _principalProviderMock = new Mock<IPrincipalProvider>();
        _auditLoggerMock = new Mock<IAuditLogger>();
        _user = TestEntities.CreateUserModel();
        _principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, _user.Id.ToString())]));
        _principalProviderMock.Setup(p => p.Principal).Returns(_principal);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region AssertGroupPermission(Guid)

    [Fact]
    public async Task AssertGroupPermission_ById_UserOwnsGroup_DoesNotThrow()
    {
        // Arrange
        var group = TestEntities.CreateGroup(ownerId: _user.Id);
        _context.Groups.Add(group);
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertGroupPermission(group.Id);
    }

    [Fact]
    public async Task AssertGroupPermission_ById_UserDoesNotOwnGroup_ThrowsNotAuthorisedException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(ownerId: otherUserId);
        _context.Groups.Add(group);
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertGroupPermission(group.Id));
    }

    [Fact]
    public async Task AssertGroupPermission_ById_GroupDoesNotExist_ThrowsNotAuthorisedException()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertGroupPermission(nonExistentGroupId));
    }

    #endregion

    #region AssertGroupPermission(Group)

    [Fact]
    public void AssertGroupPermission_ByGroup_UserOwnsGroup_DoesNotThrow()
    {
        // Arrange
        var group = TestEntities.CreateGroup(ownerId: _user.Id);
        var repository = CreateRepository();

        // Act & Assert - should not throw
        repository.AssertGroupPermission(group);
    }

    [Fact]
    public void AssertGroupPermission_ByGroup_UserDoesNotOwnGroup_ThrowsNotAuthorisedException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(ownerId: otherUserId);
        var repository = CreateRepository();

        // Act & Assert
        Assert.Throws<NotAuthorisedException>(() => repository.AssertGroupPermission(group));
    }

    #endregion

    #region AssertFamilyPermission

    [Fact]
    public async Task AssertFamilyPermission_AuthorizationSucceeds_DoesNotThrow()
    {
        // Arrange
        var familyId = _user.FamilyId;

        // Mock the underlying interface method (extension method wraps requirement in IEnumerable)
        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(_principal, familyId, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertFamilyPermission(familyId);
    }

    [Fact]
    public async Task AssertFamilyPermission_AuthorizationFails_ThrowsNotAuthorisedException()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Mock the underlying interface method
        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(_principal, familyId, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertFamilyPermission(familyId));
    }

    #endregion

    #region HasBudgetLinePermission

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineInUsersFamily_ReturnsTrueAndDoesNotAudit()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(familyId: _user.FamilyId);
        var line = TestEntities.CreateBudgetLine(budgetId: budget.Id);
        _context.Add(budget);
        _context.Add(line);
        _context.SaveChanges();

        var security = CreateBudgetLineSecurity();

        // Act
        var result = await security.HasBudgetLinePermission(line.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        _auditLoggerMock.Verify(
            a => a.AuthorizationDenied(It.IsAny<Models.User>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineInOtherFamily_ReturnsFalseAndAudits()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(familyId: Guid.NewGuid());
        var line = TestEntities.CreateBudgetLine(budgetId: budget.Id);
        _context.Add(budget);
        _context.Add(line);
        _context.SaveChanges();

        var security = CreateBudgetLineSecurity();

        // Act
        var result = await security.HasBudgetLinePermission(line.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
        _auditLoggerMock.Verify(
            a => a.AuthorizationDenied(_user, "BudgetLine", line.Id, nameof(BudgetLineRequirement)),
            Times.Once);
    }

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineNotFound_ReturnsFalseAndAudits()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var security = CreateBudgetLineSecurity();

        // Act
        var result = await security.HasBudgetLinePermission(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
        _auditLoggerMock.Verify(
            a => a.AuthorizationDenied(_user, "BudgetLine", nonExistentId, nameof(BudgetLineRequirement)),
            Times.Once);
    }

    #endregion

    #region AssertAdministrator

    [Fact]
    public async Task AssertAdministrator_AuthorizationSucceeds_DoesNotThrow()
    {
        // Arrange - Mock the underlying interface method (resource is null, policyName is the string)
        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(_principal, null, It.Is<string>(s => s == Policies.Admin)))
            .ReturnsAsync(AuthorizationResult.Success());

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertAdministrator(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAdministrator_AuthorizationFails_ThrowsNotAuthorisedException()
    {
        // Arrange - Mock the underlying interface method
        _authorizationServiceMock
            .Setup(a => a.AuthorizeAsync(_principal, null, It.Is<string>(s => s == Policies.Admin)))
            .ReturnsAsync(AuthorizationResult.Failed());

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertAdministrator(TestContext.Current.CancellationToken));
    }

    #endregion

    #region GetInstrumentIds

    [Fact]
    public async Task GetInstrumentIds_UserHasInstruments_ReturnsInstrumentIds()
    {
        // Arrange
        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        _context.InstrumentOwners.Add(TestEntities.CreateInstrumentOwner(instrumentId1, _user.Id));
        _context.InstrumentOwners.Add(TestEntities.CreateInstrumentOwner(instrumentId2, _user.Id));
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act
        var result = await repository.GetInstrumentIds(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(instrumentId1, result);
        Assert.Contains(instrumentId2, result);
    }

    [Fact]
    public async Task GetInstrumentIds_UserHasNoInstruments_ReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetInstrumentIds(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    private SecurityRepository CreateRepository() =>
        new(_context, _authorizationServiceMock.Object, _principalProviderMock.Object, _user, _auditLoggerMock.Object);

    private BudgetLineSecurity CreateBudgetLineSecurity() => new(_context, _user, _auditLoggerMock.Object);
}
