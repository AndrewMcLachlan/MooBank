using System.Globalization;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Models.Queries.Transactions;
using Asm.MooBank.Institution.Ing.Queries.Transactions;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.Ing.Importers;

public partial class IngImporter : IImporter
{
    private const int Columns = 5;
    private const int DateColumn = 0;
    private const int DescriptionColumn = 1;
    private const int CreditColumn = 2;
    private const int DebitColumn = 3;
    private const int BalanceColumn = 4;

    private readonly ITransactionExtraRepository _transactionExtraRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<IngImporter> _logger;

    public IngImporter(ITransactionExtraRepository transactionExtraRepository, ITransactionRepository transactionRepository, ILogger<IngImporter> logger)
    {
        _transactionExtraRepository = transactionExtraRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<TransactionImportResult> Import(Account account, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var transactions = new List<Transaction>();

        // Throw away header row
        await reader.ReadLineAsync(cancellationToken);

        int lineCount = 2;

        decimal? endBalance = null;

        while (!reader.EndOfStream)
        {
            DateTime transactionTime = DateTime.MinValue;
            decimal credit = 0;
            decimal debit = 0;

            string line = (await reader.ReadLineAsync(cancellationToken))!;

            string[] prelimColumns = line.Split(",");

            List<string> columns = new();

            string? current = null;

            foreach (string str in prelimColumns)
            {
                if (str.StartsWith("\"") && !str.EndsWith("\""))
                {
                    current = str;
                }
                else if (!str.StartsWith("\"") && str.EndsWith("\""))
                {
                    columns.Add((current + str).Trim('"').Replace("\"\"", "\""));
                }
                else
                {
                    columns.Add(str);
                }
            }

            #region Validation
            if (columns.Count != Columns)
            {
                _logger.LogWarning("Unrecognised entry at line {lineCount}", lineCount);
                continue;
            }

            if (!DateTime.TryParseExact(columns[DateColumn], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out transactionTime))
            {
                _logger.LogWarning("Incorrect date format at line {lineCount}", lineCount);
                continue;
            }
            if (String.IsNullOrWhiteSpace(columns[DescriptionColumn]))
            {
                _logger.LogWarning("Description not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (String.IsNullOrEmpty(columns[CreditColumn]) && String.IsNullOrEmpty(columns[DebitColumn]) || !String.IsNullOrEmpty(columns[CreditColumn]) && !String.IsNullOrEmpty(columns[DebitColumn]))
            {
                _logger.LogWarning("Credit or Debit amount not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], out credit))
            {
                _logger.LogWarning("Incorrect credit format at line {lineCount}", lineCount);
                continue;
            }
            else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], out debit))
            {
                _logger.LogWarning("Incorrect debit format at line {lineCount}", lineCount);
                continue;
            }

            TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

            if (!Decimal.TryParse(columns[BalanceColumn], out decimal balance))
            {
                _logger.LogWarning("Incorrect balance format at line {lineCount}", lineCount);
                continue;
            }
            #endregion

            endBalance ??= balance;

            var transaction = new Transaction
            {
                AccountId = account.AccountId,
                Amount = transactionType == TransactionType.Credit ? credit : debit,
                Description = columns[DescriptionColumn],
                TransactionTime = transactionTime,
                TransactionType = transactionType,
            };

            transactions.Add(transaction);
        }

        _transactionRepository.AddRange(transactions);

        //var cardNames = account.AccountAccountHolders.SelectMany(a => a.AccountHolder.Cards).ToDictionary(ac => ac.Last4Digits, ac => ac.AccountHolder.FirstName);

        var transactionExtras = new List<TransactionExtra>();

        foreach (var transaction in transactions)
        {
            TransactionExtra? extraInfo = TransactionParser.ParseDescription(transaction);

            if (extraInfo != null)
            {
                transactionExtras.Add(extraInfo);
            }

        }

        _transactionExtraRepository.AddRange(transactionExtras);

        return new TransactionImportResult(transactions, endBalance!.Value);
    }

    public async Task Reprocess(Account account, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetTransactions(account.AccountId, cancellationToken);
        var processed = await _transactionExtraRepository.GetAll(account.AccountId, cancellationToken);
        var unprocessed = await _transactionExtraRepository.GetUnprocessedTransactions(transactions, cancellationToken);

        var transactionExtras = new List<TransactionExtra>();

        processed.ToList().ForEach(e => TransactionParser.ParseDescription(e.Transaction, ref e));

        foreach (var transaction in unprocessed)
        {
            TransactionExtra? extraInfo = TransactionParser.ParseDescription(transaction);

            if (extraInfo != null)
            {
                transactionExtras.Add(extraInfo);
            }
        }

        _transactionExtraRepository.AddRange(transactionExtras);
    }

    public GetTransactionExtraDetails? CreateExtraDetailsRequest(Guid accountId, Models.PagedResult<Models.Transaction> transactions) =>
        new GetIngTransactionExtraDetails(accountId, transactions);
}
