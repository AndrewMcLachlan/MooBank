﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Assets.Models;

namespace Asm.MooBank.Modules.Assets.Commands;
public sealed record Create() : ICommand<Models.Asset>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal CurrentValue { get; init; }
    public required bool ShareWithFamily { get; init; }
    public Guid? AccountGroupId { get; init; }
}

internal class CreateHandler(IAssetRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security) :  ICommandHandler<Create, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.AccountGroupId != null)
        {
            security.AssertAccountGroupPermission(command.AccountGroupId.Value);
        }

        Domain.Entities.Asset.Asset entity = new(Guid.Empty)
        {
            Balance = command.CurrentValue,
            Name = command.Name,
            Description = command.Description,
            ShareWithFamily = command.ShareWithFamily,
            PurchasePrice = command.PurchasePrice,

        };

        entity.SetAccountHolder(user.Id);
        entity.SetAccountGroup(command.AccountGroupId, user.Id);

        repository.Add(entity);


        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
