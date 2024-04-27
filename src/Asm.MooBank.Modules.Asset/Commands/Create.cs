using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Asset.Models;

namespace Asm.MooBank.Modules.Asset.Commands;
public sealed record Create() : ICommand<Models.Asset>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal CurrentValue { get; init; }
    public required bool ShareWithFamily { get; init; }
    public Guid? AccountGroupId { get; init; }
}

internal class CreateHandler(IAssetRepository repository, IUnitOfWork unitOfWork, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.AccountGroupId != null)
        {
            Security.AssertAccountGroupPermission(command.AccountGroupId.Value);
        }

        Domain.Entities.Asset.Asset entity = new(Guid.Empty)
        {
            Balance = command.CurrentValue,
            Name = command.Name,
            Description = command.Description,
            ShareWithFamily = command.ShareWithFamily,
            PurchasePrice = command.PurchasePrice,

        };

        entity.SetAccountHolder(AccountHolder.Id);
        entity.SetAccountGroup(command.AccountGroupId, AccountHolder.Id);

        repository.Add(entity);


        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
