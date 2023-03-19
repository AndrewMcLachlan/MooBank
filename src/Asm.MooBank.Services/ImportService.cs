using Asm.Domain;
using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Importers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public interface IImportService
{
    Task Import(Guid accountId, IFormFile file, CancellationToken cancellationToken = default);
}

public class ImportService : ServiceBase, IImportService
{
    private readonly IInstitutionAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionTagRuleRepository _transactionTagRuleRepository;
    private readonly IImporterFactory _importerFactory;

    public ImportService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IInstitutionAccountRepository accountRepository, ITransactionTagRuleRepository transactionTagRuleRepository, IImporterFactory importerFactory) : base(unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _importerFactory = importerFactory;
    }

    public async Task Import(Guid accountId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.Get(accountId, cancellationToken);
        IImporter importer = await _importerFactory.Create(accountId, cancellationToken);

        using Stream stream = file.OpenReadStream();

        var importResult = await importer.Import(account, stream, cancellationToken);

        await ApplyTransactionRules(account, importResult.Transactions, cancellationToken);

        account.Balance = importResult.EndBalance;

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTransactionRules(Account account, IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        var rules = await _transactionTagRuleRepository.GetForAccount(account.AccountId);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.TransactionTags.Select(t => new TransactionTag { TransactionTagId = t.TransactionTagId })).Distinct().ToList();

            transaction.TransactionTags = applicableTags;
            //_transactionRepository.Add(transaction.Id, applicableTags);
        }
    }
}
