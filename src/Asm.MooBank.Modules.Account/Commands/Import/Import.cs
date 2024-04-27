using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Accounts.Commands.Import;

public record Import(Guid AccountId, Stream Stream) : ICommand;


internal class ImportHandler(IInstrumentRepository accountRepository, IRuleRepository ruleRepository, IImporterFactory importerFactory, IUnitOfWork unitOfWork, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Import>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;
    private readonly IRuleRepository _ruleRepository = ruleRepository;
    private readonly IImporterFactory _importerFactory = importerFactory;

    public async ValueTask Handle(Import request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out Guid accountId, out Stream stream);

        Security.AssertAccountPermission(accountId);

        var baseAccount = await _accountRepository.Get(accountId, cancellationToken) ?? throw new NotFoundException($"Account with ID {accountId} not found");

        if (baseAccount is not Domain.Entities.Account.InstitutionAccount account) throw new ArgumentException("Not a valid import account", nameof(request));

        IImporter importer = await _importerFactory.Create(accountId, cancellationToken) ?? throw new ArgumentException("Not a valid import account", nameof(request));

        var importResult = await importer.Import(account.Id, stream, cancellationToken);

        await ApplyTransactionRules(account, importResult.Transactions, cancellationToken);

        if (importResult.EndBalance is not null)
        {
            account.Balance = importResult.EndBalance.Value;
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        if (importResult.EndBalance is null)
        {
            await _accountRepository.Reload(account);
            account.Balance = account.CalculatedBalance;

            await UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ApplyTransactionRules(Instrument account, IEnumerable<Domain.Entities.Transactions.Transaction> transactions, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetForAccount(account.Id, cancellationToken);

        foreach (var transaction in transactions)
        {
            var applicableTags = rules.Where(r => transaction.Description?.Contains(r.Contains, StringComparison.OrdinalIgnoreCase) ?? false).SelectMany(r => r.Tags).Distinct(new TagEqualityComparer()).ToList();

            transaction.AddOrUpdateSplit(applicableTags);
        }
    }

}
