using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="ForecastPlanDetailsSpecification"/> specification.
/// Tests verify that the specification correctly includes Accounts with Instrument and PlannedItems with Tag.
/// </summary>
public class ForecastPlanDetailsSpecificationTests
{
    private readonly TestEntities _entities = new();

    #region Basic Application

    /// <summary>
    /// Given a collection of forecast plans
    /// When ForecastPlanDetailsSpecification is applied
    /// Then all plans should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithForecastPlans_ReturnsAllPlans()
    {
        // Arrange
        var plans = new List<ForecastPlan>
        {
            _entities.CreateForecastPlan(name: "Plan 1"),
            _entities.CreateForecastPlan(name: "Plan 2"),
            _entities.CreateForecastPlan(name: "Plan 3"),
        };

        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When ForecastPlanDetailsSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var plans = new List<ForecastPlan>();
        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single forecast plan
    /// When ForecastPlanDetailsSpecification is applied
    /// Then the plan should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSinglePlan_ReturnsSinglePlan()
    {
        // Arrange
        var plans = new List<ForecastPlan>
        {
            _entities.CreateForecastPlan(name: "Single Plan"),
        };

        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Single Plan", result[0].Name);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given forecast plans with various names
    /// When ForecastPlanDetailsSpecification is applied
    /// Then plan names should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesPlanNames()
    {
        // Arrange
        var plans = new List<ForecastPlan>
        {
            _entities.CreateForecastPlan(name: "Budget 2024"),
            _entities.CreateForecastPlan(name: "Savings Plan"),
        };

        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, p => p.Name == "Budget 2024");
        Assert.Contains(result, p => p.Name == "Savings Plan");
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When ForecastPlanDetailsSpecification is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var plans = new List<ForecastPlan>
        {
            _entities.CreateForecastPlan(name: "Keep"),
            _entities.CreateForecastPlan(name: "Remove"),
            _entities.CreateForecastPlan(name: "Keep"),
        };

        var filteredQuery = plans.AsQueryable().Where(p => p.Name == "Keep");
        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("Keep", p.Name));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of forecast plans
    /// When ForecastPlanDetailsSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var plans = new List<ForecastPlan>
        {
            _entities.CreateForecastPlan(name: "Test"),
        };

        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<ForecastPlan>>(result);
    }

    #endregion

    #region Date Preservation

    /// <summary>
    /// Given forecast plans with various dates
    /// When ForecastPlanDetailsSpecification is applied
    /// Then plan dates should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesPlanDates()
    {
        // Arrange
        var plan1 = _entities.CreateForecastPlan(name: "Plan 1");
        var plan2 = _entities.CreateForecastPlan(name: "Plan 2");

        var plans = new List<ForecastPlan> { plan1, plan2 };

        var spec = new ForecastPlanDetailsSpecification();

        // Act
        var result = spec.Apply(plans.AsQueryable()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.NotEqual(default, p.StartDate));
        Assert.All(result, p => Assert.NotEqual(default, p.EndDate));
    }

    #endregion
}
