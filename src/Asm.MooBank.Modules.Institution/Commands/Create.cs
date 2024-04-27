﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Models;

namespace Asm.MooBank.Modules.Institutions.Commands;

public sealed record Create(string Name, int InstitutionTypeId) : ICommand<Models.Institution>;


internal class CreateHandler(IInstitutionRepository repository, IUnitOfWork unitOfWork, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Institution>
{
    public async ValueTask<Models.Institution> Handle(Create command, CancellationToken cancellationToken)
    {
        await Security.AssertAdministrator(cancellationToken);

        Domain.Entities.Institution.Institution entity = new()
        {
            Name = command.Name,
            InstitutionTypeId = command.InstitutionTypeId,
        };

        repository.Add(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
