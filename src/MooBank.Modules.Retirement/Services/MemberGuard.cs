using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Retirement.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// Checks that the people and accounts a plan is being pointed at are ones the caller is entitled
/// to put in it.
/// </summary>
/// <remarks>
/// Neither is a route parameter, so no endpoint policy covers them: both arrive in the request body
/// and are checked here, which is the house pattern for command-body foreign keys.
///
/// The instrument check is the load-bearing one. A projection sums the balances of whatever
/// instruments a member is linked to, so without it a caller could name any instrument id and read
/// back its balance in the result.
/// </remarks>
public interface IMemberGuard
{
    Task Assert(IEnumerable<RetirementPlanMember> members, CancellationToken cancellationToken = default);
}

internal class MemberGuard(
    IQueryable<Domain.Entities.User.User> users,
    IQueryable<InstrumentOwner> instrumentOwners,
    MooBank.Models.User caller) : IMemberGuard
{
    public async Task Assert(IEnumerable<RetirementPlanMember> members, CancellationToken cancellationToken = default)
    {
        var list = members.ToList();
        if (list.Count == 0) return;

        await AssertPeopleAreInTheFamily(list, cancellationToken);
        await AssertAccountsBelongToTheirMember(list, cancellationToken);
    }

    private async Task AssertPeopleAreInTheFamily(List<RetirementPlanMember> members, CancellationToken cancellationToken)
    {
        // A member with nobody chosen is caught by validation before this runs, but the guard does
        // not rely on that: nobody is not somebody in the family.
        if (members.Any(m => m.UserId is null || m.UserId == Guid.Empty))
        {
            throw new NotAuthorisedException("A retirement plan can only include members of your family");
        }

        var userIds = members.Select(m => m.UserId!.Value).Distinct().ToList();

        var inFamily = await users
            .Where(u => userIds.Contains(u.Id) && u.FamilyId == caller.FamilyId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var outsider = userIds.Except(inFamily).FirstOrDefault();

        if (outsider != default)
        {
            throw new NotAuthorisedException("A retirement plan can only include members of your family");
        }
    }

    private async Task AssertAccountsBelongToTheirMember(List<RetirementPlanMember> members, CancellationToken cancellationToken)
    {
        var wanted = members
            .SelectMany(m => m.InstrumentIds.Select(i => new { UserId = m.UserId!.Value, InstrumentId = i }))
            .Distinct()
            .ToList();

        if (wanted.Count == 0) return;

        var instrumentIds = wanted.Select(w => w.InstrumentId).Distinct().ToList();

        var owned = await instrumentOwners
            .Where(o => instrumentIds.Contains(o.InstrumentId))
            .Select(o => new { o.UserId, o.InstrumentId })
            .ToListAsync(cancellationToken);

        // An account has to be owned by the very person it is being recorded against — being able
        // to see it is not enough, or one member could be credited with another's balance.
        var ownedSet = owned.Select(o => (o.UserId, o.InstrumentId)).ToHashSet();

        if (wanted.Any(w => !ownedSet.Contains((w.UserId, w.InstrumentId))))
        {
            throw new NotAuthorisedException("A superannuation account can only be recorded against the person who owns it");
        }
    }
}
