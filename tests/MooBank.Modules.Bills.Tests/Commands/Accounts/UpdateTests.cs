#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Commands.Accounts;
using Asm.MooBank.Modules.Bills.Models;
using Asm.MooBank.Modules.Bills.Tests.Support;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Modules.Bills.Tests.Commands.Accounts;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given an existing utility account
    /// When it is updated
    /// Then the editable details are written to the entity.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_UpdatesTheAccountDetails()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, new UpdateBillAccount
        {
            Name = "Renamed Electricity",
            Description = "Now on a new plan",
            AccountNumber = "ELEC999",
            ShareWithFamily = false,
        });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal("Renamed Electricity", account.Name);
        Assert.Equal("Now on a new plan", account.Description);
        Assert.Equal("ELEC999", account.AccountNumber);
        Assert.False(account.ShareWithFamily);

        Assert.Equal("Renamed Electricity", result.Name);
        Assert.Equal("ELEC999", result.AccountNumber);
        Assert.False(result.ShareWithFamily);
    }

    /// <summary>
    /// Given an existing utility account with bills against it
    /// When it is updated
    /// Then its utility type and currency are left alone, because its bills are read against them.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_LeavesUtilityTypeAndCurrencyAlone()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, new UpdateBillAccount
        {
            Name = "Renamed",
            AccountNumber = "ELEC999",
            ShareWithFamily = true,
        });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(UtilityType.Electricity, account.UtilityType);
        Assert.Equal("AUD", account.Currency);
        Assert.Equal(UtilityType.Electricity, result.UtilityType);
        Assert.Equal("AUD", result.Currency);
    }

    /// <summary>
    /// Given a description that is being cleared
    /// When the account is updated
    /// Then the description is removed rather than left in place.
    /// </summary>
    [Fact]
    public async Task Handle_NullDescription_ClearsTheDescription()
    {
        var (handler, account) = Arrange(description: "An old note");

        var command = new Update(account.Id, new UpdateBillAccount
        {
            Name = account.Name,
            Description = null,
            AccountNumber = account.AccountNumber,
            ShareWithFamily = account.ShareWithFamily,
        });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Null(account.Description);
    }

    /// <summary>
    /// Given an update
    /// When it is handled
    /// Then the instrument's caches are told to refresh.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_RaisesAnUpdatedEvent()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, new UpdateBillAccount { Name = "Renamed", AccountNumber = "ELEC999" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Contains(account.Events, e => e is Domain.Entities.Instrument.Events.InstrumentUpdatedEvent);
    }

    /// <summary>
    /// Given a valid update
    /// When it is handled
    /// Then the change is committed.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, new UpdateBillAccount { Name = "Renamed", AccountNumber = "ELEC999" });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given an account the user cannot see, or one that does not exist
    /// When it is updated
    /// Then the update is refused rather than silently doing nothing.
    /// </summary>
    [Fact]
    public async Task Handle_AccountNotFound_Throws()
    {
        var accountId = Guid.NewGuid();

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new UpdateHandler(_mocks.UnitOfWorkMock.Object, _mocks.AccountRepositoryMock.Object);

        var command = new Update(accountId, new UpdateBillAccount { Name = "Renamed", AccountNumber = "ELEC999" });

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());

        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private (UpdateHandler Handler, DomainAccount Account) Arrange(string? description = null)
    {
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(
            id: accountId,
            name: "AGL Electricity",
            description: description,
            utilityType: UtilityType.Electricity,
            accountNumber: "ELEC001",
            currency: "AUD",
            shareWithFamily: true);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        return (new UpdateHandler(_mocks.UnitOfWorkMock.Object, _mocks.AccountRepositoryMock.Object), account);
    }
}
