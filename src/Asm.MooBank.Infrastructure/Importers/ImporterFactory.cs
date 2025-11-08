using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Infrastructure.Importers;

internal class ImporterFactory(IQueryable<LogicalAccount> logicalAccounts, IServiceProvider services) : IImporterFactory
{
    public async Task<IImporter?> Create(Guid instrumentId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var logicalAccount = await logicalAccounts.Where(a => a.Id == instrumentId).Include(a => a.InstitutionAccounts).ThenInclude(a => a!.ImporterType).SingleOrDefaultAsync(cancellationToken);

        if (logicalAccount is null)
        {
            return null;
        }

        var institutionAccount = logicalAccount.InstitutionAccounts.FirstOrDefault(a => a.Id == accountId);

        var typeName = institutionAccount?.ImporterType.Type;

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
