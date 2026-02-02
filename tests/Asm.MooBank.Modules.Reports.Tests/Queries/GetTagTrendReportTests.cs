#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTagTrendReportTests
{
    private readonly TestMocks _mocks;

    public GetTagTrendReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsTagTrendReport()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;
        var tagName = "Groceries";

        var monthlyTotals = TestEntities.CreateSampleMonthlyTagTotals();
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, tagName));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
        Assert.Equal(tagId, result.TagId);
        Assert.Equal(tagName, result.TagName);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCorrectMonthCount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var monthlyTotals = TestEntities.CreateSampleMonthlyTagTotals();
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Months.Count());
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyMonths()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Months);
    }

    [Fact]
    public async Task Handle_ValidQuery_CalculatesAverage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(today.AddMonths(-2), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(today.AddMonths(-1), 200m, 180m),
            TestEntities.CreateMonthlyTagTotal(today, 300m, 270m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Average > 0);
    }

    [Fact]
    public async Task Handle_WithSmoothingFalse_DoesNotApplySmoothing()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var monthlyTotals = TestEntities.CreateSampleMonthlyTagTotals();
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = false,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Months.Count());
    }

    [Fact]
    public async Task Handle_DebitReportType_UsesDebitFilterType()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreditReportType_UsesCreditFilterType()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportRepositoryMock.Object, tags);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateCreditReportType(),
            TagId = tagId,
        };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
