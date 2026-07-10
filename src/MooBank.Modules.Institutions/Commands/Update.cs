using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Commands;

[DisplayName("UpdateInstitution")]
public sealed record Update(int Id, string Name, InstitutionType InstitutionType, int? ImporterTypeId = null) : ICommand<Models.Institution>;

internal class UpdateHandler(IInstitutionRepository repository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Update command, CancellationToken cancellationToken)
    {
        await security.AssertAdministrator();

        Domain.Entities.Institution.Institution entity = await repository.Get(command.Id, cancellationToken);

        entity.Name = command.Name;
        entity.InstitutionType = command.InstitutionType;
        entity.ImporterTypeId = command.ImporterTypeId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
