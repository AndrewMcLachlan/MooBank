using Asm.Cqrs.Queries;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.ReferenceData.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.ReferenceData.Queries;

public record GetImporterTypes() : IQuery<IEnumerable<ImporterType>>;

internal class GetImporterTypesHandler(IQueryable<Domain.Entities.ReferenceData.ImporterType> importerTypes) : IQueryHandler<GetImporterTypes, IEnumerable<ImporterType>>
{
    public async ValueTask<IEnumerable<ImporterType>> Handle(GetImporterTypes request, CancellationToken cancellationToken) =>
        await importerTypes.ToModel().ToArrayAsync(cancellationToken);
}
