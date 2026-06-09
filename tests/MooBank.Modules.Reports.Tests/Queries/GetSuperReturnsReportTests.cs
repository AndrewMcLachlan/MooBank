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

    private static MonthlyBalance Balance(DateOnly periodEnd, decimal balance) =>
        new() { PeriodEnd = periodEnd, Balance = balance };

    /// <summary>
    /// Given two balance entries at consecutive FY ends and an Employer contribution tag
    /// When the handler runs
    /// Then a single FY row is returned with implied return = closing − opening − contributions.
    /// </summary>
    [Fact]
    public async Task Handle_TwoAnnualBalances_ReturnsOneFy()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var employerTagId = 10;

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, employerTagId: employerTagId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, DateOnly.MinValue, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Balance(new DateOnly(2024, 6, 30), 100000m),
                Balance(new DateOnly(2025, 6, 30), 130000m),
            });
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, new DateOnly(2024, 7, 1), new DateOnly(2025, 6, 30), TransactionFilterType.Credit, employerTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestEntities.CreateMonthlyTagTotal(new DateOnly(2025, 1, 1), 20000m, 20000m) });

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var year = Assert.Single(result.Years);
        Assert.Equal(2025, year.FinancialYear);
        Assert.Equal(100000m, year.OpeningBalance);
        Assert.Equal(130000m, year.ClosingBalance);
        Assert.Equal(20000m, year.Contributions);
        Assert.Equal(10000m, year.Return);
        Assert.Equal(10m, year.ReturnPercent);
    }

    /// <summary>
    /// Given four annual balance entries
    /// When the handler runs
    /// Then three FY rows are returned, one per FY transition.
    /// </summary>
    [Fact]
    public async Task Handle_FourAnnualBalances_ReturnsThreeFys()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, DateOnly.MinValue, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                Balance(new DateOnly(2022, 6, 30), 50000m),
                Balance(new DateOnly(2023, 6, 30), 60000m),
                Balance(new DateOnly(2024, 6, 30), 70000m),
                Balance(new DateOnly(2025, 6, 30), 80000m),
            });

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var fys = result.Years.Select(y => y.FinancialYear).ToList();
        Assert.Equal(new[] { 2023, 2024, 2025 }, fys);
    }

    /// <summary>
    /// Given no balance entries
    /// When the handler runs
    /// Then an empty report is returned.
    /// </summary>
    [Fact]
    public async Task Handle_NoBalances_ReturnsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, DateOnly.MinValue, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Years);
    }

    /// <summary>
    /// Given a single balance entry
    /// When the handler runs
    /// Then no FYs are returned (the first computable FY needs a prior opening balance).
    /// </summary>
    [Fact]
    public async Task Handle_SingleBalance_ReturnsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalances(accountId, DateOnly.MinValue, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Balance(new DateOnly(2025, 6, 30), 100000m) });

        var handler = new GetSuperReturnsReportHandler(_mocks.ReportRepositoryMock.Object, accounts);
        var query = new GetSuperReturnsReport { AccountId = accountId };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Years);
    }
}
