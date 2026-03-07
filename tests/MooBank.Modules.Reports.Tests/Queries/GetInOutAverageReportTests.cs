#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetInOutAverageReportTests
{
    private readonly TestMocks _mocks;

    public GetInOutAverageReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsInOutReport()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var averages = TestEntities.CreateSampleCreditDebitAverages();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(averages);

        var handler = new GetInOutAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            Interval = ReportInterval.Monthly,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_ValidQuery_CalculatesAverageIncomeCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var averages = new[]
        {
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Credit, 1500m),
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Credit, 1000m),
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Debit, 800m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(averages);

        var handler = new GetInOutAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            Interval = ReportInterval.Monthly,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2500m, result.Income);
    }

    [Fact]
    public async Task Handle_ValidQuery_CalculatesAverageOutgoingsCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var averages = new[]
        {
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Credit, 1500m),
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Debit, 800m),
            TestEntities.CreateCreditDebitAverage(TransactionFilterType.Debit, 500m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(averages);

        var handler = new GetInOutAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            Interval = ReportInterval.Monthly,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1300m, result.Outgoings);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsZeroValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetInOutAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            Interval = ReportInterval.Monthly,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0m, result.Income);
        Assert.Equal(0m, result.Outgoings);
    }

    [Theory]
    [InlineData(ReportInterval.Monthly)]
    [InlineData(ReportInterval.Yearly)]
    public async Task Handle_DifferentIntervals_UsesCorrectInterval(ReportInterval interval)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitAverages(accountId, start, end, interval, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetInOutAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            Interval = interval,
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetCreditDebitAverages(accountId, start, end, interval, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
