using Asm.Cqrs.Queries;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.ReferenceData.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.ReferenceData.Queries;

public record GetInstitutionTypes() : IQuery<IEnumerable<InstitutionType>>;

internal class GetInstitutionTypesHandler(IQueryable<Domain.Entities.Institution.InstitutionType> InstitutionTypes) : IQueryHandler<GetInstitutionTypes, IEnumerable<InstitutionType>>
{
    public async ValueTask<IEnumerable<InstitutionType>> Handle(GetInstitutionTypes request, CancellationToken cancellationToken) =>
        await InstitutionTypes.ToModel().ToArrayAsync(cancellationToken);
}
