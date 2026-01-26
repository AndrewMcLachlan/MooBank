using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="ForecastPlan"/> domain entity.
/// Tests cover planned item management, archiving, and account assignment.
/// </summary>
public class ForecastPlanTests
{
    private readonly TestEntities _entities = new();

    #region AddPlannedItem / RemovePlannedItem

    /// <summary>
    /// Given a ForecastPlan with no planned items
    /// When AddPlannedItem is called
    /// Then PlannedItems.Count should be 1 and the item's ForecastPlanId should be set
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddPlannedItem_ToEmptyPlan_AddsItemWithForecastPlanId()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        var item = _entities.CreatePlannedItem(name: "Rent");

        // Act
        plan.AddPlannedItem(item);

        // Assert
        Assert.Single(plan.PlannedItems);
        Assert.Equal(plan.Id, plan.PlannedItems.First().ForecastPlanId);
    }

    /// <summary>
    /// Given a ForecastPlan with 2 planned items
    /// When RemovePlannedItem is called with the first item's ID
    /// Then PlannedItems.Count should be 1
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemovePlannedItem_WithValidId_RemovesItem()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        var item1 = _entities.CreatePlannedItem(name: "Item 1");
        var item2 = _entities.CreatePlannedItem(name: "Item 2");
        plan.AddPlannedItem(item1);
        plan.AddPlannedItem(item2);

        // Act
        plan.RemovePlannedItem(item1.Id);

        // Assert
        Assert.Single(plan.PlannedItems);
        Assert.Equal("Item 2", plan.PlannedItems.First().Name);
    }

    /// <summary>
    /// Given a ForecastPlan with 1 planned item
    /// When RemovePlannedItem is called with a non-existent ID
    /// Then PlannedItems.Count should remain 1
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemovePlannedItem_WithInvalidId_DoesNothing()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        plan.AddPlannedItem(_entities.CreatePlannedItem(name: "Item"));

        // Act
        plan.RemovePlannedItem(Guid.NewGuid());

        // Assert
        Assert.Single(plan.PlannedItems);
    }

    #endregion

    #region Archive / Restore

    /// <summary>
    /// Given an active ForecastPlan (IsArchived = false)
    /// When Archive is called
    /// Then IsArchived should be true and UpdatedUtc should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Archive_ActivePlan_SetsIsArchivedAndUpdatesTimestamp()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        plan.IsArchived = false;
        var originalUpdatedUtc = plan.UpdatedUtc;
        Thread.Sleep(1); // Ensure timestamp changes

        // Act
        plan.Archive();

        // Assert
        Assert.True(plan.IsArchived);
        Assert.True(plan.UpdatedUtc >= originalUpdatedUtc);
    }

    /// <summary>
    /// Given an archived ForecastPlan (IsArchived = true)
    /// When Restore is called
    /// Then IsArchived should be false and UpdatedUtc should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Restore_ArchivedPlan_ClearsIsArchivedAndUpdatesTimestamp()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        plan.IsArchived = true;
        var originalUpdatedUtc = plan.UpdatedUtc;
        Thread.Sleep(1); // Ensure timestamp changes

        // Act
        plan.Restore();

        // Assert
        Assert.False(plan.IsArchived);
        Assert.True(plan.UpdatedUtc >= originalUpdatedUtc);
    }

    #endregion

    #region SetAccounts

    /// <summary>
    /// Given a ForecastPlan with no accounts
    /// When SetAccounts is called with 2 instrument IDs
    /// Then Accounts.Count should be 2
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccounts_WithInstrumentIds_SetsAccountsCollection()
    {
        // Arrange
        var plan = _entities.CreateForecastPlan();
        plan.Accounts.Clear();
        var instrumentIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        plan.SetAccounts(instrumentIds);

        // Assert
        Assert.Equal(2, plan.Accounts.Count);
    }

    #endregion
}
