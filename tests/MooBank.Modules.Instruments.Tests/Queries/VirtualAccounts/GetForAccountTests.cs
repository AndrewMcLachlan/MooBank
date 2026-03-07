#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.VirtualAccounts;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.VirtualAccounts;

[Trait("Category", "Unit")]
public class GetForAccountTests
{
    private readonly TestMocks _mocks;

    public GetForAccountTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_AccountWithVirtualInstruments_ReturnsAll()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var vi1 = TestEntities.CreateVirtualInstrument(name: "Savings");
        var vi2 = TestEntities.CreateVirtualInstrument(name: "Emergency");
        var vi3 = TestEntities.CreateVirtualInstrument(name: "Holiday");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [vi1, vi2, vi3]);

        var queryable = TestEntities.CreateLogicalAccountQueryable(instrument);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_AccountWithNoVirtualInstruments_ReturnsEmptyList()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        var queryable = TestEntities.CreateLogicalAccountQueryable(instrument);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_AccountNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var queryable = TestEntities.CreateLogicalAccountQueryable([]);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(nonExistentId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_VirtualInstruments_HaveCorrectProperties()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            name: "Savings Goal",
            description: "Save for vacation",
            balance: 2500m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        var queryable = TestEntities.CreateLogicalAccountQueryable(instrument);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var first = result.First();
        Assert.Equal("Savings Goal", first.Name);
        Assert.Equal("Save for vacation", first.Description);
        Assert.Equal(2500m, first.CurrentBalance);
    }

    [Fact]
    public async Task Handle_MultipleAccounts_ReturnsOnlyRequestedAccount()
    {
        // Arrange
        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();
        var vi1 = TestEntities.CreateVirtualInstrument(name: "Account1 VI");
        var vi2 = TestEntities.CreateVirtualInstrument(name: "Account2 VI");
        var instrument1 = TestEntities.CreateInstrument(id: instrumentId1, virtualInstruments: [vi1]);
        var instrument2 = TestEntities.CreateInstrument(id: instrumentId2, virtualInstruments: [vi2]);

        var queryable = TestEntities.CreateLogicalAccountQueryable(instrument1, instrument2);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(instrumentId1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Account1 VI", result.First().Name);
    }

    [Fact]
    public async Task Handle_UsesCurrencyConverter()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            balance: 100m,
            currency: "EUR");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "EUR"))
            .Returns(165m);

        var queryable = TestEntities.CreateLogicalAccountQueryable(instrument);

        var handler = new GetForAccountHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new GetForAccount(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var first = result.First();
        Assert.Equal(100m, first.CurrentBalance);
        Assert.Equal(165m, first.CurrentBalanceLocalCurrency);
    }
}
