#nullable enable
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class GetYearsTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_BudgetsExist_ReturnsYears()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var budget2023 = TestEntities.CreateBudget(year: 2023, familyId: familyId);
        var budget2024 = TestEntities.CreateBudget(year: 2024, familyId: familyId);
        var budget2025 = TestEntities.CreateBudget(year: 2025, familyId: familyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget2023, budget2024, budget2025);

        var handler = new GetYearsHandler(queryable, _mocks.User);
        var query = new GetYears();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var years = result.ToList();
        Assert.Equal(3, years.Count);
        Assert.Contains((short)2023, years);
        Assert.Contains((short)2024, years);
        Assert.Contains((short)2025, years);
    }

    [Fact]
    public async Task Handle_NoBudgets_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateBudgetQueryable([]);

        var handler = new GetYearsHandler(queryable, _mocks.User);
        var query = new GetYears();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_BudgetsFromDifferentFamily_ReturnsOnlyUserFamilyYears()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();

        var budget2023 = TestEntities.CreateBudget(year: 2023, familyId: familyId);
        var budget2024 = TestEntities.CreateBudget(year: 2024, familyId: familyId);
        var otherBudget = TestEntities.CreateBudget(year: 2025, familyId: otherFamilyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget2023, budget2024, otherBudget);

        var handler = new GetYearsHandler(queryable, _mocks.User);
        var query = new GetYears();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var years = result.ToList();
        Assert.Equal(2, years.Count);
        Assert.Contains((short)2023, years);
        Assert.Contains((short)2024, years);
        Assert.DoesNotContain((short)2025, years);
    }

    [Fact]
    public async Task Handle_SingleBudget_ReturnsSingleYear()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var handler = new GetYearsHandler(queryable, _mocks.User);
        var query = new GetYears();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var years = result.ToList();
        Assert.Single(years);
        Assert.Equal((short)2024, years[0]);
    }

    [Fact]
    public async Task Handle_OnlyOtherFamilyBudgets_ReturnsEmptyList()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(year: 2024, familyId: otherFamilyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var handler = new GetYearsHandler(queryable, _mocks.User);
        var query = new GetYears();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }
}
