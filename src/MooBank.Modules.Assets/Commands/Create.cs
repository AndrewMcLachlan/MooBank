using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Assets.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Assets.Commands;

[DisplayName("CreateAsset")]
public sealed record Create() : ICommand<Models.Asset>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal CurrentBalance { get; init; }
    public required bool ShareWithFamily { get; init; }
    public Guid? GroupId { get; init; }
}

internal class CreateHandler(IAssetRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security, ICurrencyConverter currencyConverter) : ICommandHandler<Create, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            await security.AssertGroupPermission(command.GroupId.Value);
        }

        var entity = Domain.Entities.Asset.Asset.Create(command.Name, command.Description, command.CurrentBalance, command.ShareWithFamily, command.PurchasePrice);

        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        repository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await entity.ToModel(currencyConverter, cancellationToken);
    }
}
