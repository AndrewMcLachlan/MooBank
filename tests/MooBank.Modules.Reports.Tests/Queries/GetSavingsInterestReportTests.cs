#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

/// <summary>
/// Unit tests for the <see cref="GetSavingsInterestReportHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GetSavingsInterestReportTests
{
    private readonly TestMocks _mocks;

    public GetSavingsInterestReportTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// Given a valid query
    /// When the handler runs
    /// Then it queries the repository with the Credit filter (interest is always a credit).
    /// </summary>
    [Fact]
    public async Task Handle_AlwaysFiltersToCredit()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        var tagId = 42;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end, TagId = tagId };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Given monthly interest totals from the repository
    /// When the handler runs
    /// Then the response includes the tag name, the months, the running total, and a non-zero monthly average.
    /// </summary>
    [Fact]
    public async Task Handle_PopulatesTotalsAndAverage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);
        var tagId = 7;

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 50m, 50m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 2, 1), 75m, 75m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 3, 1), 100m, 100m),
        };
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end, TagId = tagId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(tagId, result.TagId);
        Assert.Equal("Interest", result.TagName);
        Assert.Equal(3, result.Months.Count());
        Assert.Equal(225m, result.Total);
        Assert.True(result.MonthlyAverage > 0);
    }

    /// <summary>
    /// Given no transactions tagged with the selected tag
    /// When the handler runs
    /// Then the response has empty months and zero totals.
    /// </summary>
    [Fact]
    public async Task Handle_NoData_ReturnsEmptyMonthsAndZeroTotals()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);
        var tagId = 7;

        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end, TagId = tagId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Months);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0m, result.MonthlyAverage);
    }
}
