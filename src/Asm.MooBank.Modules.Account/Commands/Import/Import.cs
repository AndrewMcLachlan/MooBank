using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Account.Commands.Import;

public record Import(Guid AccountId, Stream Stream) : ICommand;


internal class ImportHandler(IAccountRepository accountRepository, IRuleRepository ruleRepository, IImporterFactory importerFactory, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Import>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IRuleRepository _ruleRepository = ruleRepository;
    private readonly IImporterFactory _importerFactory = importerFactory;

    public async ValueTask Handle(Import request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out Guid accountId, out Stream stream);

        Security.AssertAccountPermission(accountId);

        var baseAccount = await _accountRepository.Get(accountId) ?? throw new NotFoundException($"Account with ID {accountId} not found");

        if (baseAccount is not Domain.Entities.Account.InstitutionAccount account) throw new ArgumentException("Not a valid import account", nameof(request));

        IImporter importer = await _importerFactory.Create(accountId, cancellationToken) ?? throw new ArgumentException("Not a valid import account", nameof(request));

        var importResult = await importer.Import(account, stream, cancellationToken);

        await ApplyTransactionRules(account, importResult.Transactions, cancellationToken);

        account.Balance = importResult.EndBalance;

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTransactionRules(Domain.Entities.Account.Account account, IEnumerable<Domain.Entities.Transactions.Transaction> transactions, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetForAccount(account.AccountId, cancellationToken);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.Tags).Distinct(new TagEqualityComparer()).ToList();

            transaction.AddOrUpdateSplit(applicableTags);
        }
    }

}
