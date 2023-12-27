using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class StockHoldingRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, StockHolding, Guid>(context), IStockHoldingRepository
{
}
