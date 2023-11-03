using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Modules.Account.Commands.Import;

public record Reprocess(Guid AccountId) : ICommand;

internal class ReprocessHandler : ICommandHandler<Reprocess>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInstitutionAccountRepository _institutionAccountRepository;
    private readonly ISecurity _security;
    private readonly IImporterFactory _importerFactory;

    public ReprocessHandler(IUnitOfWork unitOfWork, IInstitutionAccountRepository institutionAccountRepository, ISecurity security, IImporterFactory importerFactory)
    {
        _unitOfWork = unitOfWork;
        _institutionAccountRepository = institutionAccountRepository;
        _security = security;
        _importerFactory = importerFactory;
    }

    public async ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _institutionAccountRepository.Get(request.AccountId, cancellationToken);

        var importer = _importerFactory.Create(account.ImportAccount?.ImporterType);

        if (importer == null)
        {
            return;
        }

        await importer.Reprocess(account, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
