#nullable enable
using Asm.MooBank.Modules.Retirement.Queries;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Queries;

/// <summary>
/// Unit tests for listing and fetching retirement plans.
/// </summary>
[Trait("Category", "Unit")]
public class GetPlansTests
{
    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given plans belonging to several families
    /// When the plans are listed
    /// Then only the current user's family's plans should be returned
    /// </summary>
    [Fact]
    public async Task Handle_PlansFromOtherFamilies_AreNotReturned()
    {
        // Arrange
        var mine = TestEntities.CreatePlan(name: "Mine", familyId: _mocks.User.FamilyId);
        var theirs = TestEntities.CreatePlan(name: "Theirs", familyId: Guid.NewGuid());

        var handler = new GetPlansHandler(QueryableHelper.CreateAsyncQueryable([mine, theirs]), _mocks.User);

        // Act
        var result = await handler.Handle(new GetPlans(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Mine", Assert.Single(result).Name);
    }

    /// <summary>
    /// Given several plans
    /// When they are listed
    /// Then the most recently updated should come first
    /// </summary>
    [Fact]
    public async Task Handle_SeveralPlans_ReturnsMostRecentlyUpdatedFirst()
    {
        // Arrange
        var older = TestEntities.CreatePlan(name: "Older", familyId: _mocks.User.FamilyId);
        older.UpdatedUtc = DateTime.UtcNow.AddDays(-5);

        var newer = TestEntities.CreatePlan(name: "Newer", familyId: _mocks.User.FamilyId);
        newer.UpdatedUtc = DateTime.UtcNow;

        var handler = new GetPlansHandler(QueryableHelper.CreateAsyncQueryable([older, newer]), _mocks.User);

        // Act
        var result = await handler.Handle(new GetPlans(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Newer", "Older"], result.Select(p => p.Name));
    }

    /// <summary>
    /// Given a plan belonging to another family
    /// When it is fetched by id
    /// Then it should not be found
    /// </summary>
    /// <remarks>
    /// The endpoint is also behind the retirement plan policy; this is the query's own filtering,
    /// which is what protects any caller that does not come through that route.
    /// </remarks>
    [Fact]
    public async Task Handle_PlanFromAnotherFamily_IsNotFound()
    {
        // Arrange
        var theirs = TestEntities.CreatePlan(familyId: Guid.NewGuid());
        var handler = new GetPlanHandler(QueryableHelper.CreateAsyncQueryable([theirs]), _mocks.User);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetPlan(theirs.Id), TestContext.Current.CancellationToken).AsTask());
    }

    /// <summary>
    /// Given a plan belonging to the user's family
    /// When it is fetched by id
    /// Then it should be returned with its members
    /// </summary>
    [Fact]
    public async Task Handle_OwnPlan_IsReturnedWithMembers()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            familyId: _mocks.User.FamilyId,
            members: [TestEntities.CreateMember(name: "Self"), TestEntities.CreateMember(name: "Spouse")]);

        var handler = new GetPlanHandler(QueryableHelper.CreateAsyncQueryable([plan]), _mocks.User);

        // Act
        var result = await handler.Handle(new GetPlan(plan.Id), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(plan.Id, result.Id);
        Assert.Equal(2, result.Members.Count());
    }
}
