#nullable enable
using System.Security.Claims;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;
using SysClaims = System.Security.Claims;

namespace Asm.MooBank.Domain.Tests.Repositories;

[Trait("Category", "Integration")]
public class SecurityRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();
    private readonly Models.User _user;
    private readonly Mock<IAuthorizationService> _authorizationService;
    private readonly Mock<IPrincipalProvider> _principalProvider;
    private readonly Mock<IAuditLogger> _auditLogger;

    public SecurityRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _user = new Models.User
        {
            Id = _userId,
            EmailAddress = "test@test.com",
            FamilyId = _familyId,
            Currency = "AUD",
        };
        _authorizationService = new Mock<IAuthorizationService>();
        _principalProvider = new Mock<IPrincipalProvider>();
        _auditLogger = new Mock<IAuditLogger>();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(SysClaims.ClaimTypes.NameIdentifier, _userId.ToString())],
            "TestAuth"));
        _principalProvider.Setup(p => p.Principal).Returns(principal);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region AssertGroupPermission (Guid overload)

    [Fact]
    public async Task AssertGroupPermissionById_UserOwnsGroup_DoesNotThrow()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = new Group(groupId) { Name = "Test Group", OwnerId = _userId };
        _context.Set<Group>().Add(group);
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertGroupPermission(groupId);
    }

    [Fact]
    public async Task AssertGroupPermissionById_UserDoesNotOwnGroup_ThrowsNotAuthorisedException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var group = new Group(groupId) { Name = "Other Group", OwnerId = otherUserId };
        _context.Set<Group>().Add(group);
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertGroupPermission(groupId));
    }

    [Fact]
    public async Task AssertGroupPermissionById_GroupDoesNotExist_ThrowsNotAuthorisedException()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => repository.AssertGroupPermission(Guid.NewGuid()));
    }

    #endregion

    #region AssertGroupPermission (Group overload)

    [Fact]
    public void AssertGroupPermissionByEntity_UserOwnsGroup_DoesNotThrow()
    {
        // Arrange
        var group = new Group(Guid.NewGuid()) { Name = "Test Group", OwnerId = _userId };
        var repository = CreateRepository();

        // Act & Assert - should not throw
        repository.AssertGroupPermission(group);
    }

    [Fact]
    public void AssertGroupPermissionByEntity_UserDoesNotOwnGroup_ThrowsNotAuthorisedException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var group = new Group(Guid.NewGuid()) { Name = "Other Group", OwnerId = otherUserId };
        var repository = CreateRepository();

        // Act & Assert
        Assert.Throws<NotAuthorisedException>(() => repository.AssertGroupPermission(group));
    }

    #endregion

    #region AssertFamilyPermission

    [Fact]
    public async Task AssertFamilyPermission_UserIsFamilyMember_DoesNotThrow()
    {
        // Arrange
        _authorizationService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                _familyId,
                It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertFamilyPermission(_familyId);
    }

    [Fact]
    public async Task AssertFamilyPermission_UserIsNotFamilyMember_ThrowsNotAuthorisedException()
    {
        // Arrange
        _authorizationService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<Guid>(),
                It.IsAny<IAuthorizationRequirement[]>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() =>
            repository.AssertFamilyPermission(Guid.NewGuid()));
    }

    #endregion

    #region HasBudgetLinePermission

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineInUsersFamily_ReturnsTrueAndDoesNotAudit()
    {
        // Arrange
        var lineId = AddBudgetLine(_familyId);
        var repository = CreateRepository();

        // Act
        var result = await repository.HasBudgetLinePermission(lineId, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        _auditLogger.Verify(
            a => a.AuthorizationDenied(It.IsAny<Models.User>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineInOtherFamily_ReturnsFalseAndAudits()
    {
        // Arrange
        var lineId = AddBudgetLine(Guid.NewGuid());
        var repository = CreateRepository();

        // Act
        var result = await repository.HasBudgetLinePermission(lineId, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
        _auditLogger.Verify(
            a => a.AuthorizationDenied(_user, "BudgetLine", lineId, nameof(BudgetLineRequirement)),
            Times.Once);
    }

    [Fact]
    public async Task HasBudgetLinePermission_BudgetLineNotFound_ReturnsFalseAndAudits()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var repository = CreateRepository();

        // Act
        var result = await repository.HasBudgetLinePermission(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
        _auditLogger.Verify(
            a => a.AuthorizationDenied(_user, "BudgetLine", nonExistentId, nameof(BudgetLineRequirement)),
            Times.Once);
    }

    private Guid AddBudgetLine(Guid familyId)
    {
        var budget = new Budget(Guid.NewGuid())
        {
            FamilyId = familyId,
            Year = 2024,
        };
        var line = new BudgetLine(Guid.NewGuid())
        {
            BudgetId = budget.Id,
            TagId = 1,
            Amount = 100m,
        };

        _context.Set<Budget>().Add(budget);
        _context.Set<BudgetLine>().Add(line);
        _context.SaveChanges();

        return line.Id;
    }

    #endregion

    #region AssertAdministrator

    [Fact]
    public async Task AssertAdministrator_UserIsAdmin_DoesNotThrow()
    {
        // Arrange
        _authorizationService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                It.Is<string>(s => s == Policies.Admin)))
            .ReturnsAsync(AuthorizationResult.Success());

        var repository = CreateRepository();

        // Act & Assert - should not throw
        await repository.AssertAdministrator(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AssertAdministrator_UserIsNotAdmin_ThrowsNotAuthorisedException()
    {
        // Arrange
        _authorizationService
            .Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                null,
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() =>
            repository.AssertAdministrator(TestContext.Current.CancellationToken));
    }

    #endregion

    #region GetInstrumentIds

    [Fact]
    public async Task GetInstrumentIds_ReturnsUserOwnedInstruments()
    {
        // Arrange
        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();
        var otherInstrumentId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _context.Set<Domain.Entities.Instrument.InstrumentOwner>().AddRange(
            new Domain.Entities.Instrument.InstrumentOwner { InstrumentId = instrumentId1, UserId = _userId },
            new Domain.Entities.Instrument.InstrumentOwner { InstrumentId = instrumentId2, UserId = _userId },
            new Domain.Entities.Instrument.InstrumentOwner { InstrumentId = otherInstrumentId, UserId = otherUserId }
        );
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetInstrumentIds(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(instrumentId1, result);
        Assert.Contains(instrumentId2, result);
        Assert.DoesNotContain(otherInstrumentId, result);
    }

    #endregion

    private SecurityRepository CreateRepository() =>
        new(_context, _authorizationService.Object, _principalProvider.Object, _user, _auditLogger.Object);
}
