namespace Asm.MooBank.Domain.Entities.Instrument.Specifications;

/// <summary>
/// Filters instruments to only include those that are open (not closed) and accessible to the specified user.
/// An instrument is accessible if the user owns it, or if it's shared with family and a family member owns it.
/// </summary>
public class OpenAccessibleSpecification<T>(Guid userId, Guid? familyId) : ISpecification<T> where T : Instrument
{
    public IQueryable<T> Apply(IQueryable<T> query) =>
        query.Where(a => a.ClosedDate == null &&
            (a.Owners.Any(o => o.UserId == userId) ||
             a.ShareWithFamily && a.Owners.Any(o => o.User.FamilyId == familyId)));
}
