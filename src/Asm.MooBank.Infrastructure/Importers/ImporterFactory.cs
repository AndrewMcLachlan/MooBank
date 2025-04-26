using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Infrastructure.Importers;

internal class ImporterFactory(IQueryable<InstitutionAccount> accounts, IServiceProvider services) : IImporterFactory
{
    public async Task<IImporter?> Create(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await accounts.Where(a => a.Id == accountId && a.ImportAccount != null).Include(a => a.ImportAccount).ThenInclude(a => a!.ImporterType).SingleOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            return null;
        }

        var typeName = account.ImportAccount!.ImporterType.Type;

        if (typeName == null)
        {
            return null;
        }

        var type = Type.GetType(typeName) ?? throw new InvalidOperationException("Not a valid importer type");

        return services.GetService(type) as IImporter ?? throw new InvalidOperationException("Not a valid importer type");
    }

    public IImporter? Create(string? importerType)
    {
        if (importerType == null)
        {
            return null;
        }

        var type = Type.GetType(importerType) ?? throw new InvalidOperationException("Not a valid importer type");

        return services.GetService(type) as IImporter ?? throw new InvalidOperationException("Not a valid importer type");
    }
}
