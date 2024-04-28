using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Asset;
public class IncludeSpecification : ISpecification<Asset>
{
    public IQueryable<Asset> Apply(IQueryable<Asset> query) =>
        query.Include(s => s.Owners);
}
