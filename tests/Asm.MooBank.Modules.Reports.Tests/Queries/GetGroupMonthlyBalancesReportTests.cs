#nullable enable
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetGroupMonthlyBalancesReportTests
{
    private readonly TestMocks _mocks;

    public GetGroupMonthlyBalancesReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMonthlyBalancesReport()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var balances = TestEntities.CreateSampleMonthlyBalances();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balances);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCorrectBalanceCount()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var balances = TestEntities.CreateSampleMonthlyBalances();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balances);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Balances.Count());
    }

    [Fact]
    public async Task Handle_ValidQuery_MapsPeriodEndToMonth()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var periodEnd = new DateOnly(2024, 6, 30);

        var balances = new[]
        {
            TestEntities.CreateMonthlyBalance(periodEnd, 15000m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balances);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var trendPoint = result.Balances.Single();
        Assert.Equal(periodEnd, trendPoint.Month);
    }

    [Fact]
    public async Task Handle_ValidQuery_MapsBalanceToGrossAmount()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var expectedBalance = 25000m;

        var balances = new[]
        {
            TestEntities.CreateMonthlyBalance(balance: expectedBalance),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balances);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var trendPoint = result.Balances.Single();
        Assert.Equal(expectedBalance, trendPoint.GrossAmount);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyBalances()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Balances);
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsRepositoryWithCorrectGroupId()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetGroupMonthlyBalancesReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetGroupMonthlyBalancesReport(groupId, start, end);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetGroupMonthlyBalances(groupId, start, end, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
