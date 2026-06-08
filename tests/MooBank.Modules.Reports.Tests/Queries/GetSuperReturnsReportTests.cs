#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

/// <summary>
/// Unit tests for the <see cref="GetSuperReturnsReportHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GetSuperReturnsReportTests
{
    private readonly TestMocks _mocks;

    public GetSuperReturnsReportTests()
    {
        _mocks = new TestMocks();
    }

    private static LogicalAccount CreateAccount(Guid id, int? employerTagId = null, int? personalTagId = null)
    {
        var account = new LogicalAccount(id, []) { Name = "Test Super", Currency = "AUD" };
        if (employerTagId.HasValue) account.SetTagPurpose(TagPurpose.EmployerContribution, employerTagId.Value);
        if (personalTagId.HasValue) account.SetTagPurpose(TagPurpose.PersonalContribution, personalTagId.Value);
        return account;
    }

    /// <summary>
    /// Given a single FY range with known opening/closing balances and contributions
    /// When the handler runs
    /// Then the implied return is closing − opening − contributions.
    /// </summary>
    [Fact]
    public async Task Handle_SingleFy_ComputesImpliedReturn()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2025, 7, 1);
        var end = new DateOnly(2026, 6, 30);
        var employerTagId = 10;

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, employerTagId: employerTagId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new MonthlyBalance { PeriodEnd = new DateOnly(2025, 6, 30), Balance = 100000m },
                new MonthlyBalance { PeriodEnd = new DateOnly(2026, 6, 30), Balance = 130000m },
            });
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30), TransactionFilterType.Credit, employerTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 20000m, 20000m) });

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var year = Assert.Single(result.Years);
        Assert.Equal(2026, year.FinancialYear);
        Assert.Equal(100000m, year.OpeningBalance);
        Assert.Equal(130000m, year.ClosingBalance);
        Assert.Equal(20000m, year.Contributions);
        Assert.Equal(10000m, year.Return);
        Assert.Equal(10m, year.ReturnPercent);
    }

    /// <summary>
    /// Given a range spanning two AU FYs
    /// When the handler runs
    /// Then two rows are returned, one per FY, in chronological order.
    /// </summary>
    [Fact]
    public async Task Handle_TwoFyRange_ReturnsRowPerFy()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2025, 1, 1);
        var end = new DateOnly(2026, 3, 1);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Years.Count());
        Assert.Equal(2025, result.Years.First().FinancialYear);
        Assert.Equal(2026, result.Years.Last().FinancialYear);
    }

    /// <summary>
    /// Given an FY where the opening balance is zero
    /// When the handler runs
    /// Then ReturnPercent is null rather than producing a divide-by-zero.
    /// </summary>
    [Fact]
    public async Task Handle_ZeroOpening_LeavesReturnPercentNull()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2025, 7, 1);
        var end = new DateOnly(2026, 6, 30);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new MonthlyBalance { PeriodEnd = new DateOnly(2026, 6, 30), Balance = 5000m },
            });

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var year = Assert.Single(result.Years);
        Assert.Equal(0m, year.OpeningBalance);
        Assert.Equal(5000m, year.ClosingBalance);
        Assert.Null(year.ReturnPercent);
    }
}
