#nullable enable
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;
using DomainUser = Asm.MooBank.Domain.Entities.User.User;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the checks on the people and accounts a plan is pointed at.
/// </summary>
/// <remarks>
/// Neither arrives as a route parameter, so no endpoint policy covers them. The account check in
/// particular is what stops a caller naming any instrument id and reading its balance back out of
/// the projection.
/// </remarks>
[Trait("Category", "Unit")]
public class MemberGuardTests
{
    private static readonly Guid FamilyId = Guid.NewGuid();
    private static readonly Guid SelfId = Guid.NewGuid();
    private static readonly Guid SpouseId = Guid.NewGuid();
    private static readonly Guid OutsiderId = Guid.NewGuid();
    private static readonly Guid SelfInstrument = Guid.NewGuid();
    private static readonly Guid SpouseInstrument = Guid.NewGuid();

    private static MemberGuard CreateGuard() =>
        new(
            QueryableHelper.CreateAsyncQueryable<DomainUser>([
                new(SelfId) { EmailAddress = "self@example.com", FamilyId = FamilyId },
                new(SpouseId) { EmailAddress = "spouse@example.com", FamilyId = FamilyId },
                // Same application, different household.
                new(OutsiderId) { EmailAddress = "outsider@example.com", FamilyId = Guid.NewGuid() },
            ]),
            QueryableHelper.CreateAsyncQueryable<InstrumentOwner>([
                new() { UserId = SelfId, InstrumentId = SelfInstrument },
                new() { UserId = SpouseId, InstrumentId = SpouseInstrument },
            ]),
            TestEntities.CreateUser(SelfId, FamilyId));

    private static RetirementPlanMember Member(Guid userId, params Guid[] instrumentIds) =>
        new() { UserId = userId, CurrentAge = 45, CurrentIncome = 100_000m, RetirementAge = 65, InstrumentIds = instrumentIds };

    /// <summary>
    /// Given members who are in the caller's family, each with their own accounts
    /// When the guard runs
    /// Then it should allow them
    /// </summary>
    [Fact]
    public async Task Assert_OwnFamilyWithTheirOwnAccounts_IsAllowed()
    {
        await CreateGuard().Assert([Member(SelfId, SelfInstrument), Member(SpouseId, SpouseInstrument)], TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Given a member naming someone outside the caller's family
    /// When the guard runs
    /// Then it should be refused
    /// </summary>
    [Fact]
    public async Task Assert_PersonFromAnotherFamily_IsRefused()
    {
        await Assert.ThrowsAsync<NotAuthorisedException>(() =>
            CreateGuard().Assert([Member(OutsiderId)], TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given a member credited with an account belonging to someone else
    /// When the guard runs
    /// Then it should be refused
    /// </summary>
    /// <remarks>
    /// The pair is in the same family and the caller can see both accounts, so a family-level check
    /// would let this through — and one member would be credited with the other's balance.
    /// </remarks>
    [Fact]
    public async Task Assert_AccountBelongingToAnotherPerson_IsRefused()
    {
        await Assert.ThrowsAsync<NotAuthorisedException>(() =>
            CreateGuard().Assert([Member(SelfId, SpouseInstrument)], TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given an instrument nobody in the family owns
    /// When the guard runs
    /// Then it should be refused
    /// </summary>
    /// <remarks>
    /// The case the check exists for: a projection sums the balances of whatever instruments a
    /// member holds, so an unowned id would otherwise disclose a balance.
    /// </remarks>
    [Fact]
    public async Task Assert_AccountOwnedByNobody_IsRefused()
    {
        await Assert.ThrowsAsync<NotAuthorisedException>(() =>
            CreateGuard().Assert([Member(SelfId, Guid.NewGuid())], TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given no members at all
    /// When the guard runs
    /// Then it should allow it without querying anything
    /// </summary>
    [Fact]
    public async Task Assert_NoMembers_IsAllowed()
    {
        await CreateGuard().Assert([], TestContext.Current.CancellationToken);
    }
}
