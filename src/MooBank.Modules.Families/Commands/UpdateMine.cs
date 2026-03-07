using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Commands;

[DisplayName("UpdateMyFamily")]
public sealed record UpdateMine(UpdateFamily Family) : ICommand<Models.Family>;

internal class UpdateMineHandler(IFamilyRepository repository, IUnitOfWork unitOfWork, MooBank.Models.User user) : ICommandHandler<UpdateMine, Models.Family>
{
    public async ValueTask<Models.Family> Handle(UpdateMine command, CancellationToken cancellationToken)
    {
        var entity = await repository.Get(user.FamilyId, cancellationToken);

        entity.Name = command.Family.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
