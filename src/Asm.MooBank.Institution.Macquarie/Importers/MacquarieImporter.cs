using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.Macquarie.Domain;
using Asm.MooBank.Institution.Macquarie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.Macquarie.Importers;

internal partial class MacquarieImporter(IQueryable<TransactionRaw> rawTransactions, ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<MacquarieImporter> logger) : IImporter
{
    private const int Columns = 11;
    private const int TransactionDateColumn = 0;
    private const int DetailsColumn = 1;
    private const int AccountColumn = 2;
    private const int CategoryColumn = 3;
    private const int SubcategoryColumn = 4;
    private const int TagsColumn = 5;
    private const int NotesColumn = 6;
    private const int DebitColumn = 7;
    private const int CreditColumn = 8;
    private const int BalanceColumn = 9;
    private const int OriginalDescriptionColumn = 10;

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid instrumentId, Guid? institutionAccountId, Stream contents, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(contents);
        var rawTransactionEntities = new List<TransactionRaw>();

        var checkTransactions = await rawTransactions.Where(t => t.AccountId == instrumentId).Select(t => new
        {
            t.Details,
            t.Date,
            t.Credit,
            t.Debit,
            t.Balance,
        }).ToListAsync(cancellationToken);

        // Throw away header row
        await reader.ReadLineAsync(cancellationToken);

        int lineCount = 1;

        decimal? endBalance = null;

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            DateOnly transactionTime = DateOnly.MinValue;
            decimal credit = 0;
            decimal debit = 0;

            lineCount++;

            string[] prelimColumns = line.Split(",");

            List<string> columns = [];

            foreach (string str in prelimColumns)
            {
                columns.Add(str.Trim('"'));
            }

            #region Validation
            if (columns.Count != Columns)
            {
                logger.LogWarning("Unrecognised entry at line {lineCount} - expected {expected} columns but got {actual}", lineCount, Columns, columns.Count);
                continue;
            }

            if (!DateOnly.TryParseExact(columns[TransactionDateColumn], "dd MMM yyyy", out transactionTime))
            {
                logger.LogWarning("Incorrect date format at line {lineCount}: {date}", lineCount, columns[TransactionDateColumn]);
                continue;
            }

            if (String.IsNullOrWhiteSpace(columns[DetailsColumn]))
            {
                logger.LogWarning("Details not supplied at line {lineCount}", lineCount);
                continue;
            }

            if ((String.IsNullOrEmpty(columns[CreditColumn]) && String.IsNullOrEmpty(columns[DebitColumn])) || (!String.IsNullOrEmpty(columns[CreditColumn]) && !String.IsNullOrEmpty(columns[DebitColumn])))
            {
                logger.LogWarning("Credit or Debit amount not supplied at line {lineCount}", lineCount);
                continue;
            }

            if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], out credit))
            {
                logger.LogWarning("Incorrect credit format at line {lineCount}: {credit}", lineCount, columns[CreditColumn]);
                continue;
            }
            else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], out debit))
            {
                logger.LogWarning("Incorrect debit format at line {lineCount}: {debit}", lineCount, columns[DebitColumn]);
                continue;
            }

            TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

            // Allow pending transactions
            if (!String.IsNullOrEmpty(columns[BalanceColumn]) && !Decimal.TryParse(columns[BalanceColumn], out decimal balance))
            {
                logger.LogWarning("Incorrect balance format at line {lineCount}: {balance}", lineCount, columns[BalanceColumn]);
                continue;
            }
            else
            {
                balance = 0;
            }
            #endregion

            endBalance ??= balance;

            if (checkTransactions.Any(t => t.Details == columns[DetailsColumn] && t.Date == transactionTime && t.Debit == debit && t.Credit == credit && t.Balance == balance))
            {
                logger.LogInformation("Duplicate transaction found {details} {date}", columns[DetailsColumn], transactionTime);
                continue;
            }
            else if (checkTransactions.Any(t => t.Details == columns[DetailsColumn] && t.Date == transactionTime && t.Debit == debit && t.Credit == credit))
            {
                var existing = rawTransactions.Single(t => t.Details == columns[DetailsColumn] && t.Date == transactionTime && t.Debit == debit && t.Credit == credit);
                existing.Balance = balance;

                logger.LogInformation("Pending Transaction found and updated {details} {date}", columns[DetailsColumn], transactionTime);
                continue;
            }

            Transaction transaction = Transaction.Create(
                instrumentId,
                null, // No card holder info available yet
                transactionType == TransactionType.Credit ? credit : -Math.Abs(debit),
                GetDetails(columns[DetailsColumn], columns[SubcategoryColumn]),
                transactionTime.ToStartOfDay(),
                columns[SubcategoryColumn] == "Transfers" ? MooBank.Models.TransactionSubType.Transfer : null, // No sub-type yet
                "Macquarie Import",
                institutionAccountId,
                transactionType: transactionType
           );

            transaction.Extra = new TransactionExtra
            {
                Category = columns[CategoryColumn],
                Subcategory = columns[SubcategoryColumn],
                Tags = columns[TagsColumn],
                Notes = columns[NotesColumn],
                OriginalDescription = columns[OriginalDescriptionColumn],
            };

            var transactionRaw = new TransactionRaw
            {
                AccountId = instrumentId,
                Balance = balance,
                Credit = credit,
                Date = transactionTime,
                Debit = debit,
                Details = columns[DetailsColumn],
                Account = columns[AccountColumn],
                Category = columns[CategoryColumn],
                Subcategory = columns[SubcategoryColumn],
                Tags = columns[TagsColumn],
                Notes = columns[NotesColumn],
                OriginalDescription = columns[OriginalDescriptionColumn],
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
