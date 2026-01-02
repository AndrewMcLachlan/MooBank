using System.Globalization;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.Macquarie.Domain;
using Asm.MooBank.Institution.Macquarie.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.Macquarie.Importers;

internal partial class MacquarieImporter(IQueryable<TransactionRaw> rawTransactions, ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<MacquarieImporter> logger) : IImporter
{
    private const string DateFormat = "dd MMM yyyy";

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid instrumentId, Guid? institutionAccountId, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<MacquarieCsvRecord>();

        var rawTransactionEntities = new List<TransactionRaw>();

        var checkTransactions = await rawTransactions.Where(t => t.AccountId == instrumentId).Select(t => new
        {
            t.Details,
            t.Date,
            t.Credit,
            t.Debit,
            t.Balance,
        }).ToListAsync(cancellationToken);

        int lineCount = 1;
        decimal? endBalance = null;
        DateOnly? previousDate = null;
        int sequenceNumber = 1;

        foreach (var record in records)
        {
            lineCount++;

            #region Validation
            if (!DateOnly.TryParseExact(record.TransactionDate, DateFormat, out DateOnly transactionTime))
            {
                logger.LogWarning("Incorrect date format at line {lineCount}: {date}", lineCount, record.TransactionDate);
                continue;
            }

            if (String.IsNullOrWhiteSpace(record.Details))
            {
                logger.LogWarning("Details not supplied at line {lineCount}", lineCount);
                continue;
            }

            if ((String.IsNullOrEmpty(record.Credit) && String.IsNullOrEmpty(record.Debit)) || (!String.IsNullOrEmpty(record.Credit) && !String.IsNullOrEmpty(record.Debit)))
            {
                logger.LogWarning("Credit or Debit amount not supplied at line {lineCount}", lineCount);
                continue;
            }

            decimal credit = 0;
            decimal debit = 0;

            if (!String.IsNullOrEmpty(record.Credit) && !Decimal.TryParse(record.Credit, out credit))
            {
                logger.LogWarning("Incorrect credit format at line {lineCount}: {credit}", lineCount, record.Credit);
                continue;
            }
            else if (!String.IsNullOrEmpty(record.Debit) && !Decimal.TryParse(record.Debit, out debit))
            {
                logger.LogWarning("Incorrect debit format at line {lineCount}: {debit}", lineCount, record.Debit);
                continue;
            }

            TransactionType transactionType = !String.IsNullOrEmpty(record.Credit) ? TransactionType.Credit : TransactionType.Debit;

            if (String.IsNullOrEmpty(record.Balance))
            {
                // Assume this is a pending transaction and ignore.
                continue;
            }
            if (!Decimal.TryParse(record.Balance, out decimal balance))
            {
                logger.LogWarning("Incorrect balance format at line {lineCount}: {balance}", lineCount, record.Balance);
                continue;
            }

            #endregion

            endBalance ??= balance;

            if (checkTransactions.Any(t => t.Details == record.Details && t.Date == transactionTime && t.Debit == debit && t.Credit == credit && t.Balance == balance))
            {
                logger.LogInformation("Duplicate transaction found {details} {date}", record.Details, transactionTime);
                continue;
            }
            else if (checkTransactions.Any(t => t.Details == record.Details && t.Date == transactionTime && t.Debit == debit && t.Credit == credit))
            {
                var existing = await transactionRawRepository.GetZeroBalance(record.Details, transactionTime, debit, credit, cancellationToken);

                existing.Balance = balance;

                logger.LogInformation("Pending Transaction found and updated {details} {date}", record.Details, transactionTime);
                continue;
            }

            // Track sequence within each date - reset when date changes
            // CSV is descending, so we subtract from 59 to preserve order when sorted ascending
            if (previousDate != transactionTime)
            {
                sequenceNumber = 1;
                previousDate = transactionTime;
            }
            sequenceNumber++;

            Transaction transaction = Transaction.Create(
                instrumentId,
                null, // No card holder info available yet
                transactionType == TransactionType.Credit ? credit : -Math.Abs(debit),
                GetDetails(record.Details, record.Subcategory),
                transactionTime.ToStartOfDay(),
                record.Subcategory == "Transfers" ? MooBank.Models.TransactionSubType.Transfer : null, // No sub-type yet
                "Macquarie Import",
                institutionAccountId,
                transactionType: transactionType
           );

            transaction.Extra = new TransactionExtra
            {
                Category = record.Category,
                Subcategory = record.Subcategory,
                Tags = record.Tags,
                Notes = record.Notes,
                OriginalDescription = record.OriginalDescription,
            };

            var transactionRaw = new TransactionRaw
            {
                AccountId = instrumentId,
                Balance = balance,
                Credit = credit,
                Date = transactionTime,
                Debit = debit,
                Details = record.Details,
                Account = record.Account,
                Category = record.Category,
                Subcategory = record.Subcategory,
                Tags = record.Tags,
                Notes = record.Notes,
                OriginalDescription = record.OriginalDescription,
                SequenceNumber = sequenceNumber,
                Imported = DateTime.Now,
                Transaction = transaction,
            };

            rawTransactionEntities.Add(transactionRaw);
        }

        transactionRawRepository.AddRange(rawTransactionEntities);

        return new MooBank.Models.TransactionImportResult(rawTransactionEntities.Select(r => r.Transaction), endBalance!.Value);
    }

    public async Task Reprocess(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default)
    {
        var transactions = await transactionRepository.GetTransactions(instrumentId, institutionAccountId, cancellationToken);
        var transactionIds = transactions.Select(t => t.Id);

        var rawTransactions = await transactionRawRepository.GetAll(instrumentId, cancellationToken);
        var processed = rawTransactions.Where(t => t.TransactionId != null && transactionIds.Contains(t.TransactionId.Value));

        foreach (var raw in processed)
        {
            raw.Transaction.TransactionType = raw.Credit > 0 ? TransactionType.Credit : TransactionType.Debit;
            raw.Transaction.Amount = raw.Credit > 0 ? raw.Transaction.Amount : -Math.Abs(raw.Debit!.Value);
            raw.Transaction.Description = GetDetails(raw.Details, raw.Subcategory);
            raw.Transaction.TransactionTime = raw.Date.ToStartOfDay();
            raw.Transaction.Extra = new TransactionExtra
            {
                Category = raw.Category,
                Subcategory = raw.Subcategory,
                Tags = raw.Tags,
                Notes = raw.Notes,
                OriginalDescription = raw.OriginalDescription,
            };
        }
    }

    private static string? GetDetails(string? details, string? subcategory) =>
        details switch
        {
            "Payment" => $"{details} - {subcategory}",
            _ => details,
        };
}
