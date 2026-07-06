using System.Globalization;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.Ing.Domain;
using Asm.MooBank.Institution.Ing.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.Ing.Importers;

internal partial class IngImporter(IUserRepository accountHolderRepository, ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<IngImporter> logger) : IImporter
{
    private const int Columns = 5;
    private const int DateColumn = 0;
    private const int DescriptionColumn = 1;
    private const int CreditColumn = 2;
    private const int DebitColumn = 3;
    private const int BalanceColumn = 4;
    private const string DateFormat = "dd/MM/yyyy";

    private readonly Dictionary<short, User?> _accountHolders = [];

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid instrumentId, Guid? institutionAccountId, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
        };
        using var parser = new CsvParser(reader, config);

        // Throw away header row
        await parser.ReadAsync();

        List<string[]> rows = [];

        while (await parser.ReadAsync())
        {
            rows.Add(parser.Record ?? []);
        }

        // Only load existing raw transactions within the date range of the file, projected to the fields
        // needed for duplicate detection. Receipt numbers are parsed once, up front.
        var checkTransactions = (await GetCheckTransactions(instrumentId, rows, cancellationToken))
            .Select(t => new
            {
                t.Description,
                t.Date,
                t.Credit,
                t.Debit,
                TransactionParser.ParseDescription(t.Description).ReceiptNumber,
            })
            .ToList();

        var rawTransactionEntities = new List<TransactionRaw>();

        int lineCount = 1;

        decimal? endBalance = null;

        foreach (string[] columns in rows)
        {
            decimal credit = 0;
            decimal debit = 0;

            lineCount++;

            #region Validation
            if (columns.Length != Columns)
            {
                logger.LogWarning("Unrecognised entry at line {lineCount}", lineCount);
                continue;
            }

            if (!DateOnly.TryParseExact(columns[DateColumn], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly transactionTime))
            {
                logger.LogWarning("Incorrect date format at line {lineCount}", lineCount);
                continue;
            }
            if (String.IsNullOrWhiteSpace(columns[DescriptionColumn]))
            {
                logger.LogWarning("Description not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (String.IsNullOrEmpty(columns[CreditColumn]) && String.IsNullOrEmpty(columns[DebitColumn]) || !String.IsNullOrEmpty(columns[CreditColumn]) && !String.IsNullOrEmpty(columns[DebitColumn]))
            {
                logger.LogWarning("Credit or Debit amount not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], NumberStyles.Currency, CultureInfo.InvariantCulture, out credit))
            {
                logger.LogWarning("Incorrect credit format at line {lineCount}", lineCount);
                continue;
            }
            else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], NumberStyles.Currency, CultureInfo.InvariantCulture, out debit))
            {
                logger.LogWarning("Incorrect debit format at line {lineCount}", lineCount);
                continue;
            }

            TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

            if (!Decimal.TryParse(columns[BalanceColumn], NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal balance))
            {
                logger.LogWarning("Incorrect balance format at line {lineCount}", lineCount);
                continue;
            }
            #endregion

            endBalance ??= balance;

            var parsed = TransactionParser.ParseDescription(columns[DescriptionColumn]);

            // A receipt-number match is only valid when both sides have a receipt number.
            if (checkTransactions.Any(t => (t.Description == columns[DescriptionColumn] ||
                (t.ReceiptNumber is not null && t.ReceiptNumber == parsed.ReceiptNumber)) &&
                t.Date == transactionTime && t.Debit == debit && t.Credit == credit))
            {
                logger.LogInformation("Duplicate transaction found {description} {date}", columns[DescriptionColumn], transactionTime);
                continue;
            }

            Transaction transaction = Transaction.Create(
                instrumentId,
                (await GetAccountHolder(parsed.Last4Digits, cancellationToken))?.Id,
                transactionType == TransactionType.Credit ? credit : debit,
                parsed.Description,
                transactionTime.ToStartOfDay(),
                parsed.TransactionSubType,
                "ING Import",
                institutionAccountId
            );

            transaction.Location = parsed.Location;
            transaction.Extra = new TransactionExtra
            {
                ReceiptNumber = parsed.ReceiptNumber,
                ProcessedDate = transactionTime,
                PurchaseType = parsed.PurchaseType,
            };
            transaction.Reference = parsed.Reference;
            transaction.PurchaseDate = parsed.PurchaseDate;

            var transactionRaw = new TransactionRaw
            {
                AccountId = instrumentId,
                Balance = balance,
                Credit = credit,
                Date = transactionTime,
                Debit = debit,
                Description = columns[DescriptionColumn],
                Imported = DateTime.Now,
                Transaction = transaction,
            };

            rawTransactionEntities.Add(transactionRaw);
        }

        transactionRawRepository.AddRange(rawTransactionEntities);

        return new MooBank.Models.TransactionImportResult(rawTransactionEntities.Select(r => r.Transaction), endBalance);
    }

    public async Task Reprocess(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default)
    {
        var transactionIds = (await transactionRepository.GetTransactionIds(instrumentId, cancellationToken: cancellationToken)).ToHashSet();

        var rawTransactions = await transactionRawRepository.GetAll(instrumentId, cancellationToken);
        var processed = rawTransactions.Where(t => t.TransactionId != null && transactionIds.Contains(t.TransactionId.Value));

        foreach (var raw in processed)
        {
            var parsed = TransactionParser.ParseDescription(raw.Description);

            raw.Transaction.User = await GetAccountHolder(parsed.Last4Digits, cancellationToken);
            raw.Transaction.Description = parsed.Description;
            raw.Transaction.Location = parsed.Location;
            raw.Transaction.Extra = new TransactionExtra
            {
                ReceiptNumber = parsed.ReceiptNumber,
                ProcessedDate = raw.Date,
                PurchaseType = parsed.PurchaseType,
            };
            raw.Transaction.Reference = parsed.Reference;
            raw.Transaction.PurchaseDate = parsed.PurchaseDate;
            raw.Transaction.TransactionSubType = parsed.TransactionSubType;
            raw.Transaction.TransactionTime = raw.Date.ToStartOfDay();
        }
    }

    /// <summary>
    /// Loads the existing raw transactions that fall within the date range of the file being imported,
    /// projected down to only the fields needed for duplicate detection.
    /// </summary>
    private async Task<IReadOnlyCollection<TransactionRawSummary>> GetCheckTransactions(Guid instrumentId, IEnumerable<string[]> rows, CancellationToken cancellationToken)
    {
        List<DateOnly> dates = [];

        foreach (string[] row in rows)
        {
            if (row.Length > DateColumn && DateOnly.TryParseExact(row[DateColumn], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                dates.Add(date);
            }
        }

        if (dates.Count == 0) return [];

        return (await transactionRawRepository.GetSummaries(instrumentId, dates.Min(), dates.Max(), cancellationToken)).ToList();
    }

    private async ValueTask<User?> GetAccountHolder(short? last4Digits, CancellationToken cancellationToken)
    {
        if (last4Digits == null) return null;

        if (!_accountHolders.TryGetValue(last4Digits.Value, out User? user))
        {
            user = await accountHolderRepository.GetByCard(last4Digits.Value, cancellationToken);
            _accountHolders.Add(last4Digits.Value, user);
        }

        return user;
    }
}
