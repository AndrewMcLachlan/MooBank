#nullable enable
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Tests.Support;
using DomainFamily = Asm.MooBank.Domain.Entities.Family.Family;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Families.Tests.Commands;

[Trait("Category", "Unit")]
public class RemoveMemberTests
{
    private readonly TestMocks _mocks;

    public RemoveMemberTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesMemberFromFamily()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser, memberToRemove]);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(familyId, memberToRemove.FamilyId);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesNewFamilyForRemovedMember()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser, memberToRemove]);

        DomainFamily? capturedFamily = null;

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Add(It.IsAny<DomainFamily>()), Times.Once);
        Assert.NotNull(capturedFamily);
        Assert.Contains("John", capturedFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(firstName: "John", familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser, memberToRemove]);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RemoveSelf_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(_mocks.User.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("cannot remove yourself", exception.Message);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser]);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainUser)null!);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_UserNotInSameFamily_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser]);
        var memberInOtherFamily = TestEntities.CreateDomainUser(familyId: otherFamilyId);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberInOtherFamily.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberInOtherFamily);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(memberInOtherFamily.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("not a member of your family", exception.Message);
    }

    [Fact]
    public async Task Handle_LastMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var onlyMember = TestEntities.CreateDomainUser(familyId: familyId);
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [onlyMember]);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(onlyMember.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(onlyMember);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(onlyMember.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("last member", exception.Message);
    }

    [Fact]
    public async Task Handle_MemberWithNoFirstName_UsesEmailInFamilyName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var currentUser = TestEntities.CreateDomainUser(id: _mocks.User.Id, familyId: familyId);
        var memberToRemove = TestEntities.CreateDomainUser(familyId: familyId);
        memberToRemove.FirstName = null;
        memberToRemove.EmailAddress = "john@example.com";

        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family", members: [currentUser, memberToRemove]);

        DomainFamily? capturedFamily = null;

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(memberToRemove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberToRemove);

        var handler = new RemoveMemberHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new RemoveMember(memberToRemove.Id);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedFamily);
        Assert.Contains("john@example.com", capturedFamily.Name);
    }
}
