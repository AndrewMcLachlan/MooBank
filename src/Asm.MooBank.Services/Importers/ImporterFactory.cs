using Asm.MooBank.Importers;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Services.Importers;

public class ImporterFactory : IImporterFactory
{
    private readonly IServiceProvider _services;
    private readonly IInstitutionAccountRepository _accountRepository;

    public ImporterFactory(IInstitutionAccountRepository accountRepository, IServiceProvider services)
    {
        _accountRepository = accountRepository;
        _services = services;
    }

    public async Task<IImporter> Create(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.Get(accountId, cancellationToken);

        var typeName = account.ImportAccount?.ImporterType.Type ?? throw new InvalidOperationException("Not an import account");

        var type = Type.GetType(typeName) ?? throw new InvalidOperationException("Not a valid importer type");

        return _services.GetService(type) as IImporter ?? throw new InvalidOperationException("Not a valid importer type"); ;
    }
}
