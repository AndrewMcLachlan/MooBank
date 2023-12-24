using Asm.MooBank.Domain.Entities.Account;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Importers;

internal class ImporterFactory(IAccountRepository accountRepository, IInstitutionAccountRepository institutionAccountRepository, IServiceProvider services) : IImporterFactory
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
