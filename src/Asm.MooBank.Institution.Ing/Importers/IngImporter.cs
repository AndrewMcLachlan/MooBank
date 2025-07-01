using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Importers;
using Asm.MooBank.Institution.Ing.Domain;
using Asm.MooBank.Institution.Ing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Institution.Ing.Importers;

internal partial class IngImporter(IQueryable<TransactionRaw> rawTransactions, IUserRepository accountHolderRepository, ITransactionRawRepository transactionRawRepository, ITransactionRepository transactionRepository, ILogger<IngImporter> logger) : IImporter
{
    private const int Columns = 5;
    private const int DateColumn = 0;
    private const int DescriptionColumn = 1;
    private const int CreditColumn = 2;
    private const int DebitColumn = 3;
    private const int BalanceColumn = 4;

    private readonly Dictionary<short, User> _accountHolders = [];

    public async Task<MooBank.Models.TransactionImportResult> Import(Guid accountId, Stream contents, CancellationToken cancellationToken = default)
    {

        using var reader = new StreamReader(contents);
        var rawTransactionEntities = new List<TransactionRaw>();

        var checkTransactions = await rawTransactions.Where(t => t.AccountId == accountId).Select(t => new
        {
            t.Description,
            t.Date,
            t.Credit,
            t.Debit
        }).ToListAsync(cancellationToken);

        // Throw away header row
        await reader.ReadLineAsync(cancellationToken);

        int lineCount = 1;

        decimal? endBalance = null;

        while (!reader.EndOfStream)
        {
            DateOnly transactionTime = DateOnly.MinValue;
            decimal credit = 0;
            decimal debit = 0;

            string line = (await reader.ReadLineAsync(cancellationToken))!;
            lineCount++;

            string[] prelimColumns = line.Split(",");

            List<string> columns = [];

            string? current = null;

            foreach (string str in prelimColumns)
            {
                if (str.StartsWith('\"') && !str.EndsWith('\"'))
                {
                    current = str;
                }
                else if (!str.StartsWith('\"') && str.EndsWith('\"'))
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
                logger.LogWarning("Unrecognised entry at line {lineCount}", lineCount);
                continue;
            }

            if (!DateOnly.TryParseExact(columns[DateColumn], "dd/MM/yyyy", out transactionTime))
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

            if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], out credit))
            {
                logger.LogWarning("Incorrect credit format at line {lineCount}", lineCount);
                continue;
            }
            else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], out debit))
            {
                logger.LogWarning("Incorrect debit format at line {lineCount}", lineCount);
                continue;
            }

            TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

            if (!Decimal.TryParse(columns[BalanceColumn], out decimal balance))
            {
                logger.LogWarning("Incorrect balance format at line {lineCount}", lineCount);
                continue;
            }
            #endregion

            endBalance ??= balance;

            if (checkTransactions.Any(t => (t.Description == columns[DescriptionColumn] ||
                TransactionParser.ParseDescription(t.Description).ReceiptNumber == TransactionParser.ParseDescription(columns[DescriptionColumn]).ReceiptNumber) &&
                t.Date == transactionTime && t.Debit == debit && t.Credit == credit))
            {
                logger.LogInformation("Duplicate transaction found {description} {date}", columns[DescriptionColumn], transactionTime);
                continue;
            }

            var parsed = TransactionParser.ParseDescription(columns[DescriptionColumn]);

            var transaction = new Transaction
            {
                AccountId = accountId,
                User = parsed.Last4Digits != null ? await accountHolderRepository.GetByCard(parsed.Last4Digits.Value, cancellationToken) : null,
                Amount = transactionType == TransactionType.Credit ? credit : debit,
                Description = parsed.Description,
                Location = parsed.Location,
                Extra = new TransactionExtra
                {
                    ReceiptNumber = parsed.ReceiptNumber,
                    ProcessedDate = transactionTime,
                    PurchaseType = parsed.PurchaseType,
                },
                Reference = parsed.Reference,
                PurchaseDate = parsed.PurchaseDate,
                TransactionTime = transactionTime.ToStartOfDay(),
                TransactionType = transactionType,
                TransactionSubType = parsed.TransactionSubType,
                Source = "ING Import",
            };

            var transactionRaw = new TransactionRaw
            {
                AccountId = accountId,
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

        return new MooBank.Models.TransactionImportResult(rawTransactionEntities.Select(r => r.Transaction), endBalance!.Value);
    }

    public async Task Reprocess(Guid accountId, CancellationToken cancellationToken = default)
    {
        var transactions = await transactionRepository.GetTransactions(accountId, cancellationToken);
        var transactionIds = transactions.Select(t => t.Id);

        var rawTransactions = await transactionRawRepository.GetAll(accountId, cancellationToken);
        var processed = rawTransactions.Where(t => t.TransactionId != null && transactionIds.Contains(t.TransactionId.Value));
        var unprocessed = rawTransactions.Except(processed, new Asm.Domain.IIdentifiableEqualityComparer<TransactionRaw, Guid>());

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
        /*
        foreach (var transaction in unprocessed)
        {
            TransactionExtra? extraInfo = TransactionParser.ParseDescription(transaction);

            if (extraInfo != null)
            {
                transactionExtras.Add(extraInfo);
            }
        }*/
    }

    private async ValueTask<User?> GetAccountHolder(short? last4Digits, CancellationToken cancellationToken)
    {
        if (last4Digits == null) return null;

        if (!_accountHolders.TryGetValue(last4Digits.Value, out User? user))
        {
            user = await accountHolderRepository.GetByCard(last4Digits.Value, cancellationToken);
            if (user == null) return null;
            _accountHolders.Add(last4Digits.Value, user);
        }

        return user;
    }

    /*public GetTransactionExtraDetails? CreateExtraDetailsRequest(Guid accountId, Models.PagedResult<Models.Transaction> transactions) =>
        new GetIngTransactionExtraDetails(accountId, transactions);*/
}
