#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

/// <summary>
/// Unit tests for the <see cref="GetPrincipalVsInterestReportHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GetPrincipalVsInterestReportTests
{
    private readonly TestMocks _mocks;

    public GetPrincipalVsInterestReportTests()
    {
        _mocks = new TestMocks();
    }

    private static LogicalAccount CreateAccount(Guid id, int? interestTagId)
    {
        var account = new LogicalAccount(id, []) { Name = "Test Mortgage", Currency = "AUD" };
        if (interestTagId.HasValue) account.SetTagPurpose(TagPurpose.MortgageInterest, interestTagId.Value);
        return account;
    }

    /// <summary>
    /// Given a mortgage account with an Interest tag configured
    /// When the handler runs
    /// Then principal for each month = total debits − interest, and totals match.
    /// </summary>
    [Fact]
    public async Task Handle_TaggedInterest_DerivesPrincipalAsRemainderOfDebits()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 42;
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, tagId)]);
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Mortgage Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new MonthlyCreditDebitTotal { Month = new DateOnly(2026, 1, 1), TransactionType = TransactionFilterType.Debit, Total = 3000m },
                new MonthlyCreditDebitTotal { Month = new DateOnly(2026, 2, 1), TransactionType = TransactionFilterType.Debit, Total = 3000m },
                new MonthlyCreditDebitTotal { Month = new DateOnly(2026, 3, 1), TransactionType = TransactionFilterType.Debit, Total = 3000m },
            });
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 1500m, 1500m),
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 2, 1), 1450m, 1450m),
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 3, 1), 1400m, 1400m),
            });

        var handler = new GetPrincipalVsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetPrincipalVsInterestReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(tagId, result.InterestTagId);
        Assert.Equal("Mortgage Interest", result.InterestTagName);
        Assert.Equal(3, result.Interest.Count());
        Assert.Equal(3, result.Principal.Count());
        Assert.Equal(4350m, result.InterestTotal);
        Assert.Equal(4650m, result.PrincipalTotal);
        Assert.Equal(1500m, result.Principal.First().GrossAmount);
        Assert.Equal(1550m, result.Principal.Skip(1).First().GrossAmount);
        Assert.Equal(1600m, result.Principal.Last().GrossAmount);
    }

    /// <summary>
    /// Given an account with no Mortgage Interest tag configured
    /// When the handler runs
    /// Then an empty report is returned and the repository is never queried.
    /// </summary>
    [Fact]
    public async Task Handle_NoTagConfigured_ReturnsEmptyAndSkipsRepo()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, interestTagId: null)]);
        var tags = TestEntities.CreateTagQueryable();

        var handler = new GetPrincipalVsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetPrincipalVsInterestReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.InterestTagId);
        Assert.Empty(result.Interest);
        Assert.Empty(result.Principal);
        Assert.Equal(0m, result.InterestTotal);
        Assert.Equal(0m, result.PrincipalTotal);

        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyCreditDebitTotals(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<TransactionFilterType>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Given the period has interest-tagged debits in a month that has no total debit entry
    /// When the handler runs
    /// Then principal for that month is negative interest (i.e. the missing-debit case produces 0 − interest).
    /// </summary>
    [Fact]
    public async Task Handle_MissingTotalDebitForMonth_TreatsAsZero()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 7;
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 31);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, tagId)]);
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(tagId, "Mortgage Interest"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotals(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Debit, tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 500m, 500m) });

        var handler = new GetPrincipalVsInterestReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetPrincipalVsInterestReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(-500m, result.PrincipalTotal);
    }
}
