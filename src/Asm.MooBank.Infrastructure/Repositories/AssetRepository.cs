using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Asset;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class AssetRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, Asset, Guid>(context), IAssetRepository
{
}
