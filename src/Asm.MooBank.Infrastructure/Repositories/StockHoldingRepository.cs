using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class StockHoldingRepository : RepositoryBase<MooBankContext, StockHolding, Guid>, IStockHoldingRepository
{
    public StockHoldingRepository(MooBankContext context) : base(context)
    {
    }
}
