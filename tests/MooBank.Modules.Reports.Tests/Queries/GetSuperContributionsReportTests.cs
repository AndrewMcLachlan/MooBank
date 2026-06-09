#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

/// <summary>
/// Unit tests for the <see cref="GetSuperContributionsReportHandler"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GetSuperContributionsReportTests
{
    private readonly TestMocks _mocks;

    public GetSuperContributionsReportTests()
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
    /// Given a Super account with both Employer and Personal tags configured
    /// When the handler runs
    /// Then both series are populated and totals reflect the underlying credits.
    /// </summary>
    [Fact]
    public async Task Handle_BothTagsConfigured_PopulatesBothSeries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        var employerTagId = 10;
        var personalTagId = 20;

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, employerTagId, personalTagId)]);
        var tags = TestEntities.CreateTagQueryable(
            TestEntities.CreateTag(employerTagId, "SG Contributions"),
            TestEntities.CreateTag(personalTagId, "Personal Super"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, employerTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 1000m, 1000m),
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 2, 1), 1200m, 1200m),
            });
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, personalTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 500m, 500m),
            });

        var handler = new GetSuperContributionsReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSuperContributionsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(employerTagId, result.EmployerTagId);
        Assert.Equal("SG Contributions", result.EmployerTagName);
        Assert.Equal(personalTagId, result.PersonalTagId);
        Assert.Equal("Personal Super", result.PersonalTagName);
        Assert.Equal(2, result.Employer.Count());
        Assert.Single(result.Personal);
        Assert.Equal(2200m, result.EmployerTotal);
        Assert.Equal(500m, result.PersonalTotal);
    }

    /// <summary>
    /// Given a Super account with no tags configured
    /// When the handler runs
    /// Then an empty report is returned and the repository is never queried.
    /// </summary>
    [Fact]
    public async Task Handle_NoTagsConfigured_ReturnsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId)]);
        var tags = TestEntities.CreateTagQueryable();

        var handler = new GetSuperContributionsReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSuperContributionsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.EmployerTagId);
        Assert.Null(result.PersonalTagId);
        Assert.Empty(result.Employer);
        Assert.Empty(result.Personal);
        Assert.Equal(0m, result.EmployerTotal);
        Assert.Equal(0m, result.PersonalTotal);

        _mocks.ReportRepositoryMock.Verify(
            r => r.GetMonthlyTotalsForTag(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<TransactionFilterType>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Given a Super account with only the Employer tag configured
    /// When the handler runs
    /// Then Employer data populates and the Personal series stays empty.
    /// </summary>
    [Fact]
    public async Task Handle_OnlyEmployerConfigured_PersonalIsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        var employerTagId = 10;

        var accounts = QueryableHelper.CreateAsyncQueryable([CreateAccount(accountId, employerTagId: employerTagId)]);
        var tags = TestEntities.CreateTagQueryable(TestEntities.CreateTag(employerTagId, "Employer"));

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyTotalsForTag(accountId, start, end, TransactionFilterType.Credit, employerTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestEntities.CreateMonthlyTagTotal(new DateOnly(2026, 1, 1), 1000m, 1000m) });

        var handler = new GetSuperContributionsReportHandler(_mocks.ReportRepositoryMock.Object, accounts, tags);
        var query = new GetSuperContributionsReport { AccountId = accountId, Start = start, End = end };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(employerTagId, result.EmployerTagId);
        Assert.Null(result.PersonalTagId);
        Assert.Single(result.Employer);
        Assert.Empty(result.Personal);
    }
}
