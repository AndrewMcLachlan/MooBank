using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Commands;

[DisplayName("CreateInstitution")]
public sealed record Create(string Name, InstitutionType InstitutionType, int? ImporterTypeId = null) : ICommand<Models.Institution>;


internal class CreateHandler(IInstitutionRepository repository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Create, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Create command, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator(cancellationToken);

        Domain.Entities.Institution.Institution entity = new()
        {
            Name = command.Name,
            InstitutionType = command.InstitutionType,
            ImporterTypeId = command.ImporterTypeId,
        };

        repository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
