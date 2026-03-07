#nullable enable
using Asm.MooBank.Domain.Entities.User.Specifications;
using Asm.MooBank.Modules.Users.Commands;
using Asm.MooBank.Modules.Users.Tests.Support;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Users.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            currency: "AUD");

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var updateUser = TestEntities.CreateUpdateUser(currency: "USD");
        var command = new Update(updateUser);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesCurrency()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            currency: "AUD");

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var updateUser = TestEntities.CreateUpdateUser(currency: "EUR");
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("EUR", existingUser.Currency);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesPrimaryAccountId()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var newPrimaryAccountId = Guid.NewGuid();
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            primaryAccountId: null);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var updateUser = TestEntities.CreateUpdateUser(primaryAccountId: newPrimaryAccountId);
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(newPrimaryAccountId, existingUser.PrimaryAccountId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingUser = TestEntities.CreateDomainUser(id: userId);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var updateUser = TestEntities.CreateUpdateUser();
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_InvalidatesCache()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingUser = TestEntities.CreateDomainUser(id: userId);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var updateUser = TestEntities.CreateUpdateUser();
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.HybridCacheMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AddNewCard_AddsCardToUser()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingUser = TestEntities.CreateDomainUser(id: userId);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        var newCard = TestEntities.CreateModelUserCard(1234, "New Card");
        var updateUser = TestEntities.CreateUpdateUser(cards: [newCard]);
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(existingUser.Cards);
        Assert.Contains(existingUser.Cards, c => c.Last4Digits == 1234 && c.Name == "New Card");
    }

    [Fact]
    public async Task Handle_RemoveCard_RemovesCardFromUser()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingCard = TestEntities.CreateDomainUserCard(userId, 1234, "Existing Card");
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            cards: [existingCard]);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        // Update with no cards - should remove existing card
        var updateUser = TestEntities.CreateUpdateUser(cards: []);
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(existingUser.Cards);
    }

    [Fact]
    public async Task Handle_UpdateExistingCard_UpdatesCardName()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingCard = TestEntities.CreateDomainUserCard(userId, 1234, "Old Name");
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            cards: [existingCard]);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        // Update with same card number but different name
        var updatedCard = TestEntities.CreateModelUserCard(1234, "New Name");
        var updateUser = TestEntities.CreateUpdateUser(cards: [updatedCard]);
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(existingUser.Cards);
        var card = existingUser.Cards.Single();
        Assert.Equal(1234, card.Last4Digits);
        Assert.Equal("New Name", card.Name);
    }

    [Fact]
    public async Task Handle_MultipleCardOperations_HandlesCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var existingCards = new[]
        {
            TestEntities.CreateDomainUserCard(userId, 1111, "Card to Remove"),
            TestEntities.CreateDomainUserCard(userId, 2222, "Card to Update"),
        };
        var existingUser = TestEntities.CreateDomainUser(
            id: userId,
            cards: existingCards);

        _mocks.UserRepositoryMock
            .Setup(r => r.Get(userId, It.IsAny<GetWithCards>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new UpdateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.UserRepositoryMock.Object,
            _mocks.User,
            _mocks.HybridCacheMock.Object);

        // Remove 1111, update 2222, add 3333
        var newCards = new[]
        {
            TestEntities.CreateModelUserCard(2222, "Updated Card"),
            TestEntities.CreateModelUserCard(3333, "New Card"),
        };
        var updateUser = TestEntities.CreateUpdateUser(cards: newCards);
        var command = new Update(updateUser);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, existingUser.Cards.Count);
        Assert.DoesNotContain(existingUser.Cards, c => c.Last4Digits == 1111);
        Assert.Contains(existingUser.Cards, c => c.Last4Digits == 2222 && c.Name == "Updated Card");
        Assert.Contains(existingUser.Cards, c => c.Last4Digits == 3333 && c.Name == "New Card");
    }
}
