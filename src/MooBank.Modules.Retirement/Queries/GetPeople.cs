using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;

namespace Asm.MooBank.Modules.Retirement.Queries;

[DisplayName("GetRetirementPeople")]
public record GetPeople : IQuery<IEnumerable<RetirementPerson>>;

/// <summary>
/// The people a plan can include, and the superannuation accounts each of them owns.
/// </summary>
/// <remarks>
/// A read model of its own rather than the family endpoint, which carries members but never their
/// accounts — its user mapping is shared by every caller and does not load them. Widening that, or
/// the family query's includes, would put a join on every consumer for the sake of this one screen.
///
/// The account list is the same rule the member guard enforces on save: accounts a person owns,
/// narrowed to superannuation, so the form can only offer what the server will accept.
/// </remarks>
internal class GetPeopleHandler(
    IQueryable<Domain.Entities.User.User> users,
    IQueryable<InstrumentOwner> instrumentOwners,
    MooBank.Models.User caller) : IQueryHandler<GetPeople, IEnumerable<RetirementPerson>>
{
    public async ValueTask<IEnumerable<RetirementPerson>> Handle(GetPeople query, CancellationToken cancellationToken)
    {
        var family = await users
            .Where(u => u.FamilyId == caller.FamilyId)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.EmailAddress })
            .ToListAsync(cancellationToken);

        var userIds = family.Select(u => u.Id).ToList();

        var owned = await instrumentOwners
            .Where(o => userIds.Contains(o.UserId) && o.Instrument is Domain.Entities.Account.LogicalAccount && ((Domain.Entities.Account.LogicalAccount)o.Instrument).AccountType == AccountType.Superannuation)
            .Select(o => new { o.UserId, o.InstrumentId })
            .ToListAsync(cancellationToken);

        return family
            .Select(u => new RetirementPerson
            {
                UserId = u.Id,
                Name = String.Join(' ', new[] { u.FirstName, u.LastName }.Where(n => !String.IsNullOrWhiteSpace(n))) is { Length: > 0 } name
                    ? name
                    : u.EmailAddress,
                InstrumentIds = owned.Where(o => o.UserId == u.Id).Select(o => o.InstrumentId).ToList(),
            })
            .OrderBy(p => p.Name)
            .ToList();
    }
}
