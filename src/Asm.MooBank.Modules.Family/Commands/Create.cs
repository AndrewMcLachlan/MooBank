using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Family.Models;

namespace Asm.MooBank.Modules.Family.Commands;

public sealed record Create(string Name) : ICommand<Models.Family>;


internal class CreateHandler(IFamilyRepository repository, IUnitOfWork unitOfWork, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Family>
{
    public async ValueTask<Models.Family> Handle(Create command, CancellationToken cancellationToken)
    {
        await Security.AssertAdministrator(cancellationToken);

        Domain.Entities.Family.Family entity = new()
        {
            Name = command.Name
        };

        repository.Add(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
