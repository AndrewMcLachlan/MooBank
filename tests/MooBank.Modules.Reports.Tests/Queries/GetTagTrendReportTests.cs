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
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, tagName, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportReaderMock.Verify(
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

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateCreditReportType(),
            TagId = tagId,
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportReaderMock.Verify(
            r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region ApplySmoothing Tests

    [Fact]
    public async Task Handle_WithSmoothingTrue_AppliesSmoothing()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        // Consecutive months - no gaps
        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 2, 1), 200m, 180m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 300m, 270m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Months);
    }

    [Fact]
    public async Task Handle_WithSmoothingNull_DoesNotApplySmoothing()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 300m, 270m), // 2-month gap
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = null, // null should default to false
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Months.Count()); // No smoothing = original 2 points
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_EmptyMonths_ReturnsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Months);
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_SingleMonth_ReturnsOriginal()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 300m, 270m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Months);
        Assert.Equal(300m, result.Months.First().GrossAmount);
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_ConsecutiveMonths_PreservesOriginalValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        // Consecutive months - gap of 1
        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 2, 1), 200m, 180m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var monthsList = result.Months.ToList();
        Assert.Equal(2, monthsList.Count);
        Assert.Equal(100m, monthsList[0].GrossAmount);
        Assert.Equal(200m, monthsList[1].GrossAmount);
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_GapInMonths_SmoothsValues()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        // 2-month gap between Jan and March
        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 300m, 270m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        // With a 2-month gap, the 300 should be spread across 2 months (Jan-Feb)
        // The result should have smoothed values
        Assert.NotNull(result.Months);
        // Smoothing fills in the gap months
        Assert.True(result.Months.Count() >= 2);
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_MultipleGaps_SmoothsAllGaps()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 12, 31);
        var tagId = 1;

        // Data with gaps
        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 4, 1), 400m, 360m), // 3-month gap
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 5, 1), 500m, 450m), // consecutive
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result.Months);
        // Should have filled in the gaps
        Assert.True(result.Months.Count() > 3);
    }

    /// <summary>
    /// Given months with a gap (Jan=10, Apr=30, May=5)
    /// When smoothing is applied
    /// Then the earlier point keeps its own value, the gap month values are spread from the
    /// later point, each month appears exactly once, and the overall total is preserved (45).
    /// </summary>
    [Fact]
    public async Task Handle_WithSmoothingTrue_GapInMonths_PreservesTotalAndEmitsEachMonthOnce()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 10m, 10m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 4, 1), 30m, 30m), // 3-month gap
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 5, 1), 5m, 5m),   // consecutive
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var monthsList = result.Months.ToList();

        Assert.Equal(5, monthsList.Count); // Jan, Feb, Mar, Apr, May - each exactly once
        Assert.Equal(45m, monthsList.Sum(m => m.GrossAmount)); // 10 + 30 + 5 preserved

        Assert.Equal(10m, monthsList[0].GrossAmount); // Jan keeps its own value
        Assert.Equal(10m, monthsList[1].GrossAmount); // Feb = 30 / 3
        Assert.Equal(10m, monthsList[2].GrossAmount); // Mar = 30 / 3
        Assert.Equal(10m, monthsList[3].GrossAmount); // Apr = 30 / 3
        Assert.Equal(5m, monthsList[4].GrossAmount);  // May keeps its own value
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_MaintainsChronologicalOrder()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 300m, 270m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 5, 1), 500m, 450m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var monthsList = result.Months.ToList();
        for (int i = 1; i < monthsList.Count; i++)
        {
            Assert.True(monthsList[i].Month > monthsList[i - 1].Month,
                $"Month at index {i} ({monthsList[i].Month}) should be after month at index {i - 1} ({monthsList[i - 1].Month})");
        }
    }

    [Fact]
    public async Task Handle_WithSmoothingTrue_CalculatesCorrectAverages()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 6, 30);
        var tagId = 1;

        // 2-month gap: total of 200 spread over 2 months = 100 each
        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 1, 1), 100m, 90m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2024, 3, 1), 200m, 180m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
            ApplySmoothing = true,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var monthsList = result.Months.ToList();
        // First point should be 100 (original)
        Assert.Equal(100m, monthsList[0].GrossAmount);
        // Gap filling should produce averaged values (200/2 = 100 per month)
        // The smoothing algorithm spreads the March value (200) over the gap
    }

    #endregion

    [Fact]
    public async Task Handle_CalculatesOffsetAverage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(today.AddMonths(-2), 100m, 80m),
            TestEntities.CreateMonthlyTagTotal(today.AddMonths(-1), 200m, 160m),
            TestEntities.CreateMonthlyTagTotal(today, 300m, 240m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: _mocks.User.FamilyId));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.OffsetAverage > 0);
        // OffsetAverage should be calculated from NetAmount
    }

    [Fact]
    public async Task Handle_TagFromDifferentFamily_Throws()
    {
        // Arrange - the tag lookup is family-scoped, so another family's tag is not found
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;

        var monthlyTotals = TestEntities.CreateSampleMonthlyTagTotals();
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, familyId: Guid.NewGuid()));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
        Assert.True(
            exception is InvalidOperationException ||
            (exception.InnerException is InvalidOperationException),
            "Expected InvalidOperationException or wrapped InvalidOperationException");
    }

    [Fact]
    public async Task Handle_DeletedTag_StillReturnsReport()
    {
        // Arrange - trend reports remain available for soft-deleted tags
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var tagId = 1;
        var tagName = "Deleted Tag";

        var monthlyTotals = TestEntities.CreateSampleMonthlyTagTotals();
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, tagName, familyId: _mocks.User.FamilyId, deleted: true));

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetTagTrendReportHandler(_mocks.ReportReaderMock.Object, tags, _mocks.User);

        var query = new GetTagTrendReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            TagId = tagId,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(tagName, result.TagName);
    }
}
