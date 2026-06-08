#nullable enable
using Asm.MooBank.Domain.Entities.Account;
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

    private static LogicalAccount CreateAccount(Guid id, int? interestTagId)
    {
        var account = new LogicalAccount(id, [])
        {
            Name = "Test Savings",
            Currency = "AUD",
        };
        if (interestTagId.HasValue)
        {
            account.SetTagPurpose(TagPurpose.Interest, interestTagId.Value);
        }
        return account;
    }

    /// <summary>
    /// Given a Savings account with an Interest tag configured
    /// When the handler runs
    /// Then it queries the repository with the Credit filter and the configured tag.
    /// </summary>
    [Fact]
    public async Task Handle_ConfiguredInterestTag_FiltersToCreditWithThatTag()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 42;
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, tagId)]);
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end };

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
    public async Task Handle_ConfiguredInterestTag_PopulatesTotalsAndAverage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 7;
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, tagId)]);
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Interest"));

        var monthlyTotals = new[]
        {
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 50m, 50m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 2, 1), 75m, 75m),
            TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 3, 1), 100m, 100m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monthlyTotals);

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end };

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
    /// Given an account with no Interest tag configured
    /// When the handler runs
    /// Then it returns an empty report and does NOT hit the repository for monthly totals.
    /// </summary>
    [Fact]
    public async Task Handle_NoInterestTagConfigured_ReturnsEmptyReportWithoutRepositoryCall()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, interestTagId: null)]);
        var tags = TestEntities.CreateTagQueryable();

        var handler = new GetSavingsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSavingsInterestReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.TagId);
        Assert.Null(result.TagName);
        Assert.Empty(result.Months);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0m, result.MonthlyAverage);

        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<TransactionFilterType>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
