#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTagAverageReportTests
{
    private readonly TestMocks _mocks;

    public GetAllTagAverageReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsAllTagAverageReport()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagAverages = TestEntities.CreateSampleTagAverages();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagAverages);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 10,
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
    public async Task Handle_ValidQuery_ReturnsTagsFromRepository()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagAverages = TestEntities.CreateSampleTagAverages();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagAverages);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 10,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Tags.Count());
    }

    [Fact]
    public async Task Handle_TopLimitApplied_ReturnsOnlyRequestedCount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagAverages = new[]
        {
            TestEntities.CreateTagAverage(1, "Tag1", 500m),
            TestEntities.CreateTagAverage(2, "Tag2", 400m),
            TestEntities.CreateTagAverage(3, "Tag3", 300m),
            TestEntities.CreateTagAverage(4, "Tag4", 200m),
            TestEntities.CreateTagAverage(5, "Tag5", 100m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagAverages);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 3,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Tags.Count());
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyTags()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 10,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Tags);
    }

    [Theory]
    [InlineData(ReportInterval.Monthly)]
    [InlineData(ReportInterval.Yearly)]
    public async Task Handle_DifferentIntervals_UsesCorrectInterval(ReportInterval interval)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, interval, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = interval,
            Top = 10,
        };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.ReportRepositoryMock.Verify(
            r => r.GetTopTagAverages(accountId, start, end, interval, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuery_IncludesReportTypeInResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateCreditReportType(),
            Interval = ReportInterval.Monthly,
            Top = 10,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(TransactionFilterType.Credit, (TransactionFilterType)result.ReportType);
    }

    [Fact]
    public async Task Handle_TopIsZero_ReturnsEmpty()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var tagAverages = TestEntities.CreateSampleTagAverages();

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagAverages);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 0, // Edge case: zero top
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task Handle_ResultOrderingWithTake_ReturnsFirstNResults()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6));
        var end = DateOnly.FromDateTime(DateTime.Today);

        // Repository returns tags in a specific order
        var tagAverages = new[]
        {
            TestEntities.CreateTagAverage(1, "First", 1000m),
            TestEntities.CreateTagAverage(2, "Second", 500m),
            TestEntities.CreateTagAverage(3, "Third", 250m),
            TestEntities.CreateTagAverage(4, "Fourth", 100m),
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetTopTagAverages(accountId, start, end, ReportInterval.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagAverages);

        var handler = new GetAllTagAverageReportHandler(_mocks.ReportRepositoryMock.Object);

        var query = new GetAllTagAverageReport
        {
            AccountId = accountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
            Interval = ReportInterval.Monthly,
            Top = 2, // Only take first 2
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var tags = result.Tags.ToList();
        Assert.Equal(2, tags.Count);
        Assert.Equal("First", tags[0].TagName);
        Assert.Equal("Second", tags[1].TagName);
    }
}
