using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;

namespace Asm.MooBank.Services;

public interface IImportService
{
    Task Import(Guid accountId, Stream stream, CancellationToken cancellationToken = default);
}

public class ImportService : ServiceBase, IImportService
{
    private readonly IInstitutionAccountRepository _accountRepository;
    private readonly IRuleRepository _transactionTagRuleRepository;
    private readonly IImporterFactory _importerFactory;

    public ImportService(IUnitOfWork unitOfWork, IInstitutionAccountRepository accountRepository, IRuleRepository transactionTagRuleRepository, IImporterFactory importerFactory) : base(unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _importerFactory = importerFactory;
    }

    public async Task Import(Guid accountId, Stream stream, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.Get(accountId, cancellationToken);
        IImporter importer = await _importerFactory.Create(accountId, cancellationToken) ?? throw new ArgumentException("Not a valid import account", nameof(accountId));

        var importResult = await importer.Import(account, stream, cancellationToken);

        await ApplyTransactionRules(account, importResult.Transactions, cancellationToken);

        account.Balance = importResult.EndBalance;

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTransactionRules(Account account, IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        var rules = await _transactionTagRuleRepository.GetForAccount(account.AccountId, cancellationToken);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.Tags).Distinct(new TagEqualityComparer()).ToList();

            transaction.AddOrUpdateSplit(applicableTags);
        }
    }
}
