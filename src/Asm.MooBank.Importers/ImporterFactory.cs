using Asm.MooBank.Domain.Entities.ReferenceData;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Importers;

internal class ImporterFactory : IImporterFactory
{
    private readonly IServiceProvider _services;
    private readonly IInstitutionAccountRepository _accountRepository;

    public ImporterFactory(IInstitutionAccountRepository accountRepository, IServiceProvider services)
    {
        _accountRepository = accountRepository;
        _services = services;
    }

    public async Task<IImporter?> Create(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.Get(accountId, cancellationToken);

        var typeName = account.ImportAccount?.ImporterType.Type;

        if (typeName == null)
        {
            return null;
        }

        var type = Type.GetType(typeName) ?? throw new InvalidOperationException("Not a valid importer type");

        return _services.GetService(type) as IImporter ?? throw new InvalidOperationException("Not a valid importer type"); ;
    }

    public IImporter? Create(ImporterType? importerType)
    {
        var typeName = importerType?.Type;

        if (typeName == null)
        {
            return null;
        }

        var type = Type.GetType(typeName) ?? throw new InvalidOperationException("Not a valid importer type");

        return _services.GetService(type) as IImporter ?? throw new InvalidOperationException("Not a valid importer type"); ;
    }
}
