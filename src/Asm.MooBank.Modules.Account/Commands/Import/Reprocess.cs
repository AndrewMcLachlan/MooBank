using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Modules.Account.Commands.Import;

public record Reprocess(Guid AccountId) : ICommand;

internal class ReprocessHandler(IUnitOfWork unitOfWork, IInstitutionAccountRepository institutionAccountRepository, ISecurity security, IImporterFactory importerFactory) : ICommandHandler<Reprocess>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IInstitutionAccountRepository _institutionAccountRepository = institutionAccountRepository;
    private readonly ISecurity _security = security;
    private readonly IImporterFactory _importerFactory = importerFactory;

    public async ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _institutionAccountRepository.Get(request.AccountId, cancellationToken);

        var importer = _importerFactory.Create(account.ImportAccount?.ImporterType.Type);

        if (importer == null)
        {
            return;
        }

        await importer.Reprocess(request.AccountId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
