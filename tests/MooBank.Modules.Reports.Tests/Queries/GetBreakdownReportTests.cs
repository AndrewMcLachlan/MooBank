#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetBreakdownReportTests
{
    private readonly TestMocks _mocks;

    public GetBreakdownReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsBreakdownReport()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagTotals = TestEntities.CreateSampleTransactionTagTotals();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagTotals);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
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
    public async Task Handle_ValidQuery_ReturnsTagsFromRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagTotals = TestEntities.CreateSampleTransactionTagTotals();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagTotals);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Tags.Count());
    }

    [Fact]
    public async Task Handle_WithParentTagId_PassesParentTagIdToRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var parentTagId = 42;

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, parentTagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            ParentTagId = parentTagId,
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, parentTagId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyTags()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task Handle_DebitReportType_UsesDebitFilterType()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Debit, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreditReportType_UsesCreditFilterType()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Credit, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetBreakdownReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetBreakdownReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateCreditReportType(),
        };

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetTransactionTagTotals(
                accountId, start, end, TransactionFilterType.Credit, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
