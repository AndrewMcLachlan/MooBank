#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetInOutReportTests
{
    private readonly TestMocks _mocks;

    public GetInOutReportTests()
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

        var totals = TestEntities.CreateSampleCreditDebitTotals();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_ValidQuery_CalculatesIncomeCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var totals = new[]
        {
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Credit, 3000m),
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Credit, 2000m),
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Debit, 1500m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(5000m, result.Income);
    }

    [Fact]
    public async Task Handle_ValidQuery_CalculatesOutgoingsCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var totals = new[]
        {
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Credit, 3000m),
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Debit, 1500m),
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Debit, 1000m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2500m, result.Outgoings);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsZeroValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(0m, result.Income);
        Assert.Equal(0m, result.Outgoings);
    }

    [Fact]
    public async Task Handle_OnlyCredits_ReturnsZeroOutgoings()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var totals = new[]
        {
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Credit, 5000m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(5000m, result.Income);
        Assert.Equal(0m, result.Outgoings);
    }

    [Fact]
    public async Task Handle_OnlyDebits_ReturnsZeroIncome()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var totals = new[]
        {
            TestEntities.CreateCreditDebitTotal(TransactionFilterType.Debit, 3500m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totals);

        var handler = new GetInOutReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetInOutReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(0m, result.Income);
        Assert.Equal(3500m, result.Outgoings);
    }
}
