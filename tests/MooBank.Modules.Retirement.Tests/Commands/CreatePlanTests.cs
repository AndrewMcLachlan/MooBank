#nullable enable
using Asm.MooBank.Modules.Retirement.Commands;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Tests.Support;
using DomainPlan = Asm.MooBank.Domain.Entities.Retirement.RetirementPlan;

namespace Asm.MooBank.Modules.Retirement.Tests.Commands;

/// <summary>
/// Unit tests for creating a retirement plan.
/// </summary>
[Trait("Category", "Unit")]
public class CreatePlanTests
{
    private readonly TestMocks _mocks = new();

    private CreatePlanHandler CreateHandler() =>
        new(_mocks.RetirementRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

    private static RetirementPlanBase CreateRequest(IEnumerable<RetirementPlanMember>? members = null) =>
        new()
        {
            Name = "Retirement",
            ExpectedReturnRate = 0.065m,
            InflationRate = 0.025m,
            SuperGuaranteeRate = 0.12m,
            ContributionsTaxRate = 0.15m,
            LifeExpectancy = 90,
            Members = members ?? [],
        };

    /// <summary>
    /// Given a new plan
    /// When it is created
    /// Then it should be added to the repository and saved
    /// </summary>
    [Fact]
    public async Task Handle_NewPlan_IsAddedAndSaved()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        await handler.Handle(new CreatePlan(CreateRequest()), TestContext.Current.CancellationToken);

        // Assert
        _mocks.RetirementRepositoryMock.Verify(r => r.Add(It.IsAny<DomainPlan>()), Times.Once);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a new plan
    /// When it is created
    /// Then it should belong to the current user's family
    /// </summary>
    [Fact]
    public async Task Handle_NewPlan_BelongsToTheCurrentUsersFamily()
    {
        // Arrange
        DomainPlan? added = null;
        _mocks.RetirementRepositoryMock.Setup(r => r.Add(It.IsAny<DomainPlan>())).Callback<DomainPlan>(p => added = p);

        var handler = CreateHandler();

        // Act
        await handler.Handle(new CreatePlan(CreateRequest()), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(added);
        Assert.Equal(_mocks.User.FamilyId, added.FamilyId);
    }

    /// <summary>
    /// Given a plan with assumptions
    /// When it is created
    /// Then those assumptions should be stored on the plan
    /// </summary>
    [Fact]
    public async Task Handle_NewPlan_StoresTheAssumptions()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new CreatePlan(CreateRequest()), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0.065m, result.ExpectedReturnRate);
        Assert.Equal(0.025m, result.InflationRate);
        Assert.Equal(0.12m, result.SuperGuaranteeRate);
        Assert.Equal(0.15m, result.ContributionsTaxRate);
        Assert.Equal(90, result.LifeExpectancy);
    }

    /// <summary>
    /// Given a plan with members
    /// When it is created
    /// Then each member and their accounts should be added
    /// </summary>
    [Fact]
    public async Task Handle_PlanWithMembers_AddsEachMemberAndTheirAccounts()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var request = CreateRequest([
            new RetirementPlanMember
            {
                Name = "Self",
                DateOfBirth = new DateOnly(1980, 5, 1),
                CurrentIncome = 120_000m,
                RetirementAge = 65,
                InstrumentIds = [instrumentId],
            },
            new RetirementPlanMember
            {
                Name = "Spouse",
                DateOfBirth = new DateOnly(1982, 9, 12),
                CurrentIncome = 90_000m,
                RetirementAge = 67,
                InstrumentIds = [],
            },
        ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new CreatePlan(request), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Members.Count());

        var self = result.Members.Single(m => m.Name == "Self");
        Assert.Equal(new DateOnly(1980, 5, 1), self.DateOfBirth);
        Assert.Equal(120_000m, self.CurrentIncome);
        Assert.Equal(65, self.RetirementAge);
        Assert.Equal([instrumentId], self.InstrumentIds);

        Assert.Empty(result.Members.Single(m => m.Name == "Spouse").InstrumentIds);
    }

    /// <summary>
    /// Given a member whose instrument appears more than once
    /// When the plan is created
    /// Then the instrument should only be linked once
    /// </summary>
    /// <remarks>
    /// The link table has a uniqueness constraint on member and instrument, so a duplicate would
    /// otherwise fail at the database rather than in the domain.
    /// </remarks>
    [Fact]
    public async Task Handle_DuplicateInstrument_IsLinkedOnce()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var request = CreateRequest([
            new RetirementPlanMember
            {
                Name = "Self",
                DateOfBirth = new DateOnly(1980, 5, 1),
                RetirementAge = 65,
                InstrumentIds = [instrumentId, instrumentId],
            },
        ]);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new CreatePlan(request), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([instrumentId], result.Members.Single().InstrumentIds);
    }
}
