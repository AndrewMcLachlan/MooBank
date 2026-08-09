#nullable enable
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

/// <summary>
/// Unit tests for recording which payments belong to a planned item.
/// </summary>
/// <remarks>
/// Owning the plan is not the same as owning the payments. The candidate list only ever offers
/// spending on the plan's own accounts; this command has to hold the same line, or an identifier
/// that could never have been offered can still be linked and read back as a figure.
/// </remarks>
[Trait("Category", "Unit")]
public class SetPlannedItemPaymentsTests
{
    private readonly TestMocks _mocks = new();

    private (SetPlannedItemPaymentsHandler Handler, DomainPlannedItem Item) CreateHandler()
    {
        var planId = Guid.NewGuid();
        var item = TestEntities.CreatePlannedItem(planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: _mocks.User.FamilyId, plannedItems: [item]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        return (new SetPlannedItemPaymentsHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User), item);
    }

    /// <summary>
    /// Given payments on the plan's own accounts
    /// When they are linked to an item
    /// Then they should be recorded
    /// </summary>
    [Fact]
    public async Task Handle_PaymentsInScope_RecordsThem()
    {
        // Arrange
        var (handler, item) = CreateHandler();
        var payments = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        await handler.Handle(
            new SetPlannedItemPayments(item.ForecastPlanId, item.Id, new PlannedItemPayments { TransactionIds = payments }),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(payments.Order(), item.Transactions.Select(t => t.TransactionId).Order());
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a payment that is not on any of the plan's accounts
    /// When it is linked
    /// Then it should be refused and nothing recorded
    /// </summary>
    /// <remarks>
    /// The defect this pins down: the command took whatever identifiers it was handed. Owning the
    /// plan was the only thing checked, so somebody else's payment could be linked to your own plan
    /// and its amount read back through the forecast.
    /// </remarks>
    [Fact]
    public async Task Handle_PaymentOutOfScope_IsRefusedAndRecordsNothing()
    {
        // Arrange
        var (handler, item) = CreateHandler();
        var stranger = Guid.NewGuid();

        _mocks.PlannedItemMatcherMock
            .Setup(m => m.FindOutOfScope(It.IsAny<IEnumerable<Guid>>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([stranger]);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new SetPlannedItemPayments(item.ForecastPlanId, item.Id, new PlannedItemPayments { TransactionIds = [stranger] }),
            TestContext.Current.CancellationToken).AsTask());

        Assert.Empty(item.Transactions);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given an empty list of payments
    /// When it is applied
    /// Then the item's links should be cleared without complaint
    /// </summary>
    /// <remarks>
    /// Unlinking everything is how an author says a payment was not the item's after all, so it
    /// must not be mistaken for an attempt to link nothing they own.
    /// </remarks>
    [Fact]
    public async Task Handle_EmptyList_ClearsTheLinks()
    {
        // Arrange
        var (handler, item) = CreateHandler();

        await handler.Handle(
            new SetPlannedItemPayments(item.ForecastPlanId, item.Id, new PlannedItemPayments { TransactionIds = [Guid.NewGuid()] }),
            TestContext.Current.CancellationToken);

        // Act
        await handler.Handle(
            new SetPlannedItemPayments(item.ForecastPlanId, item.Id, new PlannedItemPayments { TransactionIds = [] }),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(item.Transactions);
    }
}
