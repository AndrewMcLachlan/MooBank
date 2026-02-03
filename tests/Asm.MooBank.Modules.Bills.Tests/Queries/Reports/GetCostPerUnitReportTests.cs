#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Queries.Reports;
using Asm.MooBank.Modules.Bills.Tests.Support;

namespace Asm.MooBank.Modules.Bills.Tests.Queries.Reports;

[Trait("Category", "Unit")]
public class GetCostPerUnitReportTests
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
            pricePerUnit: 0.25m,
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

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
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

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
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
        var period1 = TestEntities.CreatePeriod(pricePerUnit: 0.20m);
        var period2 = TestEntities.CreatePeriod(pricePerUnit: 0.30m);
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

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
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
        var gasAccount = TestEntities.CreateAccountWithOwner(
            name: "Gas",
            ownerId: userId,
            utilityType: UtilityType.Gas,
            bills: [bill2]);

        var queryable = TestEntities.CreateAccountQueryable(elecAccount, gasAccount);

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31),
            UtilityType = UtilityType.Electricity
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result.DataPoints, dp => Assert.Equal("Electricity", dp.AccountName));
    }

    [Fact]
    public async Task Handle_DataPointsGroupedByDateAndAccount()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var period1 = TestEntities.CreatePeriod(
            periodEnd: new DateTime(2024, 1, 31),
            pricePerUnit: 0.20m,
            totalUsage: 100);
        var period2 = TestEntities.CreatePeriod(
            periodEnd: new DateTime(2024, 1, 31),
            pricePerUnit: 0.30m,
            totalUsage: 200);
        var bill = TestEntities.CreateBill(
            id: 1,
            issueDate: new DateOnly(2024, 2, 1),
            periods: [period1, period2]);
        var account = TestEntities.CreateAccountWithOwner(
            name: "Test Account",
            ownerId: userId,
            bills: [bill]);

        var queryable = TestEntities.CreateAccountQueryable(account);

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        // Two periods with same date should be grouped
        var dataPoint = result.DataPoints.Single(dp => dp.Date == new DateOnly(2024, 1, 31));
        Assert.Equal(300, dataPoint.TotalUsage); // 100 + 200
        Assert.Equal(0.25m, dataPoint.AveragePricePerUnit); // (0.20 + 0.30) / 2
    }

    [Fact]
    public async Task Handle_OrdersByDateThenAccountName()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var periodA = TestEntities.CreatePeriod(periodEnd: new DateTime(2024, 2, 28));
        var periodB = TestEntities.CreatePeriod(periodEnd: new DateTime(2024, 1, 31));
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

        var handler = new GetCostPerUnitReportHandler(queryable, _mocks.User);
        var query = new GetCostPerUnitReport
        {
            Start = new DateOnly(2024, 1, 1),
            End = new DateOnly(2024, 12, 31)
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var dataPoints = result.DataPoints.ToList();
        Assert.True(dataPoints[0].Date <= dataPoints[1].Date);
    }
}
