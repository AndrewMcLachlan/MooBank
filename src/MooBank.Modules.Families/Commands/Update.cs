using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Families.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Families.Commands;

[DisplayName("UpdateFamily")]
public sealed record Update([FromRoute] Guid Id, [FromBody] UpdateFamily Family) : ICommand<Models.Family>
{
}

internal class UpdateHandler(IFamilyRepository repository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, Models.Family>
{
    public async ValueTask<Models.Family> Handle(Update command, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator(cancellationToken);

        Domain.Entities.Family.Family entity = await repository.Get(command.Id, cancellationToken);

        entity.Name = command.Family.Name;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
