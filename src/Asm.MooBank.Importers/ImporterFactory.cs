using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.ReferenceData;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Importers;

internal class ImporterFactory : IImporterFactory
{
    private readonly IServiceProvider _services;
    private readonly IAccountRepository _accountRepository;
    private readonly IInstitutionAccountRepository _institutionAccountRepository;

    public ImporterFactory(IAccountRepository accountRepository, IInstitutionAccountRepository institutionAccountRepository, IServiceProvider services)
    {
        _accountRepository = accountRepository;
        _institutionAccountRepository = institutionAccountRepository;
        _services = services;
    }

    public async Task<IImporter?> Create(Guid accountId, CancellationToken cancellationToken = default)
    {
        var baseAccount = await _accountRepository.Get(accountId, cancellationToken);

        if (baseAccount is not Domain.Entities.Account.InstitutionAccount account)
        {
            return null;
        }

        await _institutionAccountRepository.Load(account, cancellationToken);

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
