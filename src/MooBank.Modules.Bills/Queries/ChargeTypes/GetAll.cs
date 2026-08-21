using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.ChargeTypes;

public record GetAll() : IQuery<IEnumerable<ChargeType>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Utility.ChargeType> chargeTypes) : IQueryHandler<GetAll, IEnumerable<ChargeType>>
{
    public async ValueTask<IEnumerable<ChargeType>> Handle(GetAll query, CancellationToken cancellationToken) =>
        (await chargeTypes.OrderBy(c => c.Name).ToListAsync(cancellationToken)).ToModel();
}
