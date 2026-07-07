#nullable enable
using Asm.MooBank.Domain.Entities.Family.Specifications;
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Tests.Support;
using DomainFamily = Asm.MooBank.Domain.Entities.Family.Family;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Families.Tests.Commands;

/// <summary>
/// Unit tests for the <see cref="RemoveMemberHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
public class RemoveMemberTests
{
    private readonly TestMocks _mocks;

    public RemoveMemberTests()
    {
        _mocks = new TestMocks();
    }

    private RemoveMemberHandler CreateHandler() => new(
        _mocks.FamilyRepositoryMock.Object,
        _mocks.UserRepositoryMock.Object,
        _mocks.UnitOfWorkMock.Object,
        _mocks.User);

    private DomainFamily SetupFamily(params DomainUser[] members)
    {
        var family = new DomainFamily
        {
            Name = "Test Family",
        };

        foreach (var member in members)
        {
            family.AccountHolders.Add(member);
        }

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(_mocks.User.FamilyId, It.IsAny<GetWithMembers>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        return family;
    }

    /// <summary>
    /// Given a family with two members
    /// When one member is removed
    /// Then the member is moved out of the family.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_RemovesMemberFromFamily()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);

        SetupFamily(currentUser, memberToRemove);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = CreateHandler();

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(familyId, memberToRemove.FamilyId);
    }

    /// <summary>
    /// Given a family with two members
    /// When one member is removed
    /// Then the family is fetched with its members loaded so the member count check works.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_FetchesFamilyWithMembers()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);

        SetupFamily(currentUser, memberToRemove);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = CreateHandler();

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(
            r => r.Get(familyId, It.IsAny<GetWithMembers>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a family with two members
    /// When one member is removed
    /// Then a new family is created for the removed member.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_CreatesNewFamilyForRemovedMember()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);

        SetupFamily(currentUser, memberToRemove);

        DomainFamily? capturedFamily = null;

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = CreateHandler();

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Add(It.IsAny<DomainFamily>()), Times.Once);
        Assert.NotNull(capturedFamily);
        Assert.Contains("John", capturedFamily.Name);
    }

    /// <summary>
    /// Given a family with two members
    /// When one member is removed
    /// Then the changes are saved.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);

        SetupFamily(currentUser, memberToRemove);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = CreateHandler();

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given the current user
    /// When they attempt to remove themselves
    /// Then an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [Fact]
    public async Task Handle_RemoveSelf_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = CreateHandler();

        var command = new RemoveMember(_mocks.User.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("cannot remove yourself", exception.Message);
    }

    /// <summary>
    /// Given a user that does not exist
    /// When removal is attempted
    /// Then a <see cref="NotFoundException"/> is thrown.
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);

        SetupFamily(currentUser);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainUser)null!);

        var handler = CreateHandler();

        var command = new RemoveMember(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    /// <summary>
    /// Given a user that belongs to a different family
    /// When removal is attempted
    /// Then an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [Fact]
    public async Task Handle_UserNotInSameFamily_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberInOtherFamily = TestEntities.CreateDomainUser(familyId: otherFamilyId);

        SetupFamily(currentUser);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberInOtherFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberInOtherFamily);

        var handler = CreateHandler();

        var command = new RemoveMember(memberInOtherFamily.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("not a member of your family", exception.Message);
    }

    /// <summary>
    /// Given a family with a single member
    /// When removal of that member is attempted
    /// Then an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [Fact]
    public async Task Handle_LastMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var onlyMember = TestEntities.CreateDomainUser(familyId: familyId);

        SetupFamily(onlyMember);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(onlyMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(onlyMember);

        var handler = CreateHandler();

        var command = new RemoveMember(onlyMember.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("last member", exception.Message);
    }

    /// <summary>
    /// Given a member without a first name
    /// When they are removed
    /// Then their email address is used to name the new family.
    /// </summary>
    [Fact]
    public async Task Handle_MemberWithNoFirstName_UsesEmailInFamilyName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(familyId: familyId);
        memberToRemove.FirstName = null;
        memberToRemove.EmailAddress = "john@example.com";

        SetupFamily(currentUser, memberToRemove);

        DomainFamily? capturedFamily = null;

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = CreateHandler();

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedFamily);
        Assert.Contains("john@example.com", capturedFamily.Name);
    }
}
