#nullable enable
using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Modules.Retirement.Commands;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Tests.Support;
using DomainPlan = Asm.MooBank.Domain.Entities.Retirement.RetirementPlan;

namespace Asm.MooBank.Modules.Retirement.Tests.Commands;

/// <summary>
/// Unit tests for updating a retirement plan, in particular reconciling its members.
/// </summary>
[Trait("Category", "Unit")]
public class UpdatePlanTests
{
    private readonly TestMocks _mocks = new();

    private UpdatePlanHandler CreateHandler(DomainPlan plan)
    {
        _mocks.RetirementRepositoryMock
            .Setup(r => r.Get(plan.Id, It.IsAny<RetirementPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        return new UpdatePlanHandler(_mocks.RetirementRepositoryMock.Object, _mocks.MemberGuardMock.Object, _mocks.UnitOfWorkMock.Object);
    }

    private static RetirementPlanBase Request(string name = "Updated", IEnumerable<RetirementPlanMember>? members = null) =>
        new()
        {
            Name = name,
            ExpectedReturnRate = 0.07m,
            InflationRate = 0.03m,
            SuperGuaranteeRate = 0.12m,
            ContributionsTaxRate = 0.15m,
            LifeExpectancy = 92,
            Members = members ?? [],
        };

    private static RetirementPlanMember Member(Guid? id = null, Guid? userId = null, int retirementAge = 65, IEnumerable<Guid>? instrumentIds = null) =>
        new()
        {
            Id = id,
            UserId = userId ?? Guid.NewGuid(),
            CurrentAge = 45,
            CurrentIncome = 100_000m,
            RetirementAge = retirementAge,
            InstrumentIds = instrumentIds ?? [],
        };

    /// <summary>
    /// Given an existing plan
    /// When it is updated
    /// Then the new assumptions should be applied and saved
    /// </summary>
    [Fact]
    public async Task Handle_ExistingPlan_AppliesTheNewAssumptions()
    {
        // Arrange
        var plan = TestEntities.CreatePlan();
        var handler = CreateHandler(plan);

        // Act
        var result = await handler.Handle(new UpdatePlan(plan.Id, Request()), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Updated", result.Name);
        Assert.Equal(0.07m, result.ExpectedReturnRate);
        Assert.Equal(92, result.LifeExpectancy);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a member supplied without an id
    /// When the plan is updated
    /// Then that member should be added
    /// </summary>
    [Fact]
    public async Task Handle_MemberWithoutAnId_IsAdded()
    {
        // Arrange
        var plan = TestEntities.CreatePlan();
        var handler = CreateHandler(plan);
        var spouseUserId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new UpdatePlan(plan.Id, Request(members: [Member(userId: spouseUserId)])), TestContext.Current.CancellationToken);

        // Assert
        // Identified by user: the display name comes from the user record, which a member added in
        // this request has not loaded.
        Assert.Equal(spouseUserId, result.Members.Single().UserId);
    }

    /// <summary>
    /// Given an existing member supplied with changes
    /// When the plan is updated
    /// Then that member should be updated in place rather than replaced
    /// </summary>
    [Fact]
    public async Task Handle_ExistingMember_IsUpdatedInPlace()
    {
        // Arrange
        var existing = TestEntities.CreateMember(name: "Self", retirementAge: 65);
        var plan = TestEntities.CreatePlan(members: [existing]);
        var handler = CreateHandler(plan);

        var instrumentId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(
            new UpdatePlan(plan.Id, Request(members: [Member(existing.Id, userId: existing.UserId, retirementAge: 60, instrumentIds: [instrumentId])])),
            TestContext.Current.CancellationToken);

        // Assert
        var member = result.Members.Single();
        Assert.Equal(existing.Id, member.Id);
        Assert.Equal(60, member.RetirementAge);
        Assert.Equal([instrumentId], member.InstrumentIds);
    }

    /// <summary>
    /// Given an existing member the caller left out
    /// When the plan is updated
    /// Then that member should be removed
    /// </summary>
    [Fact]
    public async Task Handle_MemberLeftOut_IsRemoved()
    {
        // Arrange
        var kept = TestEntities.CreateMember(name: "Kept");
        var dropped = TestEntities.CreateMember(name: "Dropped");
        var plan = TestEntities.CreatePlan(members: [kept, dropped]);
        var handler = CreateHandler(plan);

        // Act
        var result = await handler.Handle(
            new UpdatePlan(plan.Id, Request(members: [Member(kept.Id, userId: kept.UserId)])),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(kept.UserId, result.Members.Single().UserId);
    }

    /// <summary>
    /// Given every member left out
    /// When the plan is updated
    /// Then the plan should be left with no members
    /// </summary>
    [Fact]
    public async Task Handle_AllMembersLeftOut_RemovesThemAll()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [TestEntities.CreateMember(), TestEntities.CreateMember()]);
        var handler = CreateHandler(plan);

        // Act
        var result = await handler.Handle(new UpdatePlan(plan.Id, Request()), TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Members);
    }

    /// <summary>
    /// Given a member id that belongs to a different plan
    /// When the plan is updated
    /// Then it should fail rather than quietly create a new member
    /// </summary>
    [Fact]
    public async Task Handle_MemberIdFromAnotherPlan_Throws()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [TestEntities.CreateMember()]);
        var handler = CreateHandler(plan);

        var command = new UpdatePlan(plan.Id, Request(members: [Member(Guid.NewGuid())]));

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
