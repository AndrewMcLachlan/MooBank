using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Families.Models;

namespace Asm.MooBank.Modules.Families.Commands;

[DisplayName("CreateFamily")]
public sealed record Create(string Name) : ICommand<Models.Family>;


internal class CreateHandler(IFamilyRepository repository, IUnitOfWork unitOfWork, ISecurity security) :  ICommandHandler<Create, Models.Family>
{
    public async ValueTask<Models.Family> Handle(Create command, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator(cancellationToken);

        Domain.Entities.Family.Family entity = new()
        {
            Name = command.Name
        };

        repository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
