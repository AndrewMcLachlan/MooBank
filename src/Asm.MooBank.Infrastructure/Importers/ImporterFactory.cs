using Asm.MooBank.Domain.Entities.Account;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Importers;

internal class ImporterFactory(IInstrumentRepository accountRepository, IInstitutionAccountRepository institutionAccountRepository, IServiceProvider services) : IImporterFactory
{
    public async Task<IImporter?> Create(Guid accountId, CancellationToken cancellationToken = default)
    {
        var baseAccount = await accountRepository.Get(accountId, cancellationToken);

        if (baseAccount is not InstitutionAccount account)
        {
            return null;
        }

        await institutionAccountRepository.Load(account, cancellationToken);

        var typeName = account.ImportAccount?.ImporterType.Type;

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
