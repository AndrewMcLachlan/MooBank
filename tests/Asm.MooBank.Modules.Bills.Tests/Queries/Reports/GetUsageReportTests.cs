#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Reports;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Reports;

[Trait("Category", "Unit")]
public class GetUsageReportTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_BillsInDateRange_ReturnsDataPoints()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 1),
            periodEnd: new DateTime(2024, 1, 31),
            totalUsage: 500);
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2024, 2, 1),
            periods: [period]);
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(result.DataPoints);
        Assert.Equal(new DateOnly(2024, 1, 1), result.Start);
        Assert.Equal(new DateOnly(2024, 12, 31), result.End);
    }

    [Fact]
    public async Task Handle_NoBillsInDateRange_ReturnsEmptyDataPoints()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period = TestEntities.CreatePeriod();
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2023, 6, 1),
            periods: [period]);
        var account = TestEntities.CreateAccountWithOwner(ownerId: userId, bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.DataPoints);
    }

    [Fact]
    public async Task Handle_AccountIdFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var targetAccountId = Guid.NewGuid();
        var period1 = TestEntities.CreatePeriod(totalUsage: 100);
        var period2 = TestEntities.CreatePeriod(totalUsage: 200);
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 15), periods: [period1]);
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 1, 15), periods: [period2]);
        var account1 = TestEntities.CreateAccountWithOwner(
            id: targetAccountId,
            name: "Target Account",
            ownerId: userId,
            bills: [bill1]);
        var account2 = TestEntities.CreateAccountWithOwner(
            name: "Other Account",
            ownerId: userId,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(account1, account2);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31),
            AccountId = targetAccountId
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result.DataPoints, dp => Assert.Equal("Target Account", dp.AccountName));
    }

    [Fact]
    public async Task Handle_UtilityTypeFilter_FiltersCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period1 = TestEntities.CreatePeriod();
        var period2 = TestEntities.CreatePeriod();
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 1, 15), periods: [period1]);
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 1, 15), periods: [period2]);
        var elecAccount = TestEntities.CreateAccountWithOwner(
            name: "Electricity",
            ownerId: userId,
            utilityType: UtilityType.Electricity,
            bills: [bill1]);
        var waterAccount = TestEntities.CreateAccountWithOwner(
            name: "Water",
            ownerId: userId,
            utilityType: UtilityType.Water,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(elecAccount, waterAccount);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31),
            UtilityType = UtilityType.Water
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result.DataPoints, dp => Assert.Equal("Water", dp.AccountName));
    }

    [Fact]
    public async Task Handle_CalculatesUsagePerDay()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 1),
            periodEnd: new DateTime(2024, 1, 31),
            totalUsage: 300); // 300 usage over 30 days = 10 per day
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2024, 2, 1),
            periods: [period]);
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var dataPoint = result.DataPoints.First();
        Assert.Equal(10m, dataPoint.UsagePerDay); // 300 / 30 days
    }

    [Fact]
    public async Task Handle_DataPointsGroupedByDateAndAccount()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period1 = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 1),
            periodEnd: new DateTime(2024, 1, 31),
            totalUsage: 300); // 30 days
        var period2 = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 16),
            periodEnd: new DateTime(2024, 1, 31),
            totalUsage: 150); // 15 days
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2024, 2, 1),
            periods: [period1, period2]);
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var dataPoint = result.DataPoints.Single(dp => dp.Date == new DateOnly(2024, 1, 31));
        // Total usage = 300 + 150 = 450, total days = 30 + 15 = 45, usage per day = 10
        Assert.Equal(10m, dataPoint.UsagePerDay);
    }

    [Fact]
    public async Task Handle_ZeroDaysPeriod_ExcludedFromResults()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var zeroDaysPeriod = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 15),
            periodEnd: new DateTime(2024, 1, 15), // Same day
            totalUsage: 100);
        var validPeriod = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 1),
            periodEnd: new DateTime(2024, 1, 31),
            totalUsage: 300);
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2024, 2, 1),
            periods: [zeroDaysPeriod, validPeriod]);
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        // Should only have datapoints for valid period, not zero-days period
        Assert.Single(result.DataPoints);
    }

    [Fact]
    public async Task Handle_OrdersByDateThenAccountName()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var periodA = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 2, 1),
            periodEnd: new DateTime(2024, 2, 28));
        var periodB = TestEntities.CreatePeriod(
            periodStart: new DateTime(2024, 1, 1),
            periodEnd: new DateTime(2024, 1, 31));
        var bill1 = TestEntities.CreateBill(id: 1, issueDate: new DateOnly(2024, 3, 1), periods: [periodA]);
        var bill2 = TestEntities.CreateBill(id: 2, issueDate: new DateOnly(2024, 2, 1), periods: [periodB]);
        var accountA = TestEntities.CreateAccountWithOwner(
            name: "Account A",
            ownerId: userId,
            bills: [bill1]);
        var accountB = TestEntities.CreateAccountWithOwner(
            name: "Account B",
            ownerId: userId,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(accountA, accountB);

        var handler = new GetUsageReportHandler(queryable, _mocks.User);
        var query = new GetUsageReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.Equal(2, dataPoints.Count);
        Assert.True(dataPoints[0].Date <= dataPoints[1].Date);
    }
}
