using System.Globalization;
using System.Text.RegularExpressions;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Importers;
using Asm.MooBank.Models.Queries.Transactions;
using Asm.MooBank.Models.Queries.Transactions.Ing;
using Microsoft.Extensions.Logging;
using TransactionType = Asm.MooBank.Models.TransactionType;

namespace Asm.MooBank.Services.Importers
{
    public partial class IngImporter : IImporter
    {
        private const int Columns = 5;
        private const int DateColumn = 0;
        private const int DescriptionColumn = 1;
        private const int CreditColumn = 2;
        private const int DebitColumn = 3;
        private const int BalanceColumn = 4;

        [GeneratedRegex("^(.+) - Visa Purchase - Receipt (\\d{6})In (.+) Date (.+) Card (462263xxxxxx\\d{4})")]
        private static partial Regex VisaPurchase();
        [GeneratedRegex("^(.+) - Direct Debit - Receipt (\\d{6}) (.+)")]
        private static partial Regex DirectDebit();
        [GeneratedRegex("^(.+) - Internal Transfer - Receipt (\\d{6}) (.*)")]
        private static partial Regex InternalTransfer();
        [GeneratedRegex("^(.+) - EFTPOS Purchase - Receipt (\\d{6})Date (.+) Card (462263xxxxxx\\d{4})")]
        private static partial Regex EftposPurchase();
        [GeneratedRegex("^([^-]+) - Receipt (\\d{6})")]
        private static partial Regex DirectPayment();

        //private static readonly Regex VisaPurchase = VisaPurchaseRegEx();
        //private static readonly Regex DirectDebit = DirectDebit();
        //private static readonly Regex InternalTransfer = InternalTransfer();
        //private static readonly Regex EftposPurchase = EftposPurchase();
        //private static readonly Regex DirectPayment = DirectPayment();

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

            var transactionExtras = new List<TransactionExtra>();

            foreach (var transaction in transactions)
            {
                TransactionExtra? extraInfo = ParseDescription(transaction);

                if (extraInfo != null)
                {
                    transactionExtras.Add(extraInfo);
                }

            }

            _transactionExtraRepository.AddRange(transactionExtras);

            return new TransactionImportResult(transactions, endBalance!.Value);
        }

        private static TransactionExtra? ParseDescription(Transaction transaction)
        {
            var description = transaction.Description ?? String.Empty;

            var match = VisaPurchase().Match(description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    Transaction = transaction,
                    Description = match.Groups[1].Value,
                    Location = match.Groups[3].Value,
                    PurchaseDate = DateTime.Parse(match.Groups[4].Value),
                    PurchaseType = "Visa",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                };
            }

            match = DirectDebit().Match(description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    Transaction = transaction,
                    Description = match.Groups[1].Value,
                    PurchaseType = "Direct Debit",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    Reference = match.Groups[3].Value,
                };
            }

            match = InternalTransfer().Match(description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    Transaction = transaction,
                    Description = match.Groups[1].Value,
                    PurchaseType = "Internal Transfer",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    Reference = match.Groups[3].Value
                };
            }

            match = EftposPurchase().Match(description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    Transaction = transaction,
                    Description = match.Groups[1].Value,
                    PurchaseDate = DateTime.ParseExact(match.Groups[3].Value, "dd MMM yyyy Ti\\me h:mmtt", CultureInfo.CurrentCulture),
                    PurchaseType = "Visa",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                };
            }

            match = DirectPayment().Match(description);
            {
                if (match.Success)
                {
                    return new TransactionExtra
                    {
                        Transaction = transaction,
                        Description = match.Groups[1].Value,
                        PurchaseType = "Direct",
                        ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    };
                }
            }

            return null;
        }

        public GetTransactionExtraDetails? CreateExtraDetailsRequest(Models.PagedResult<Models.Transaction> transactions) =>
            new GetIngTransactionExtraDetails(transactions);
    }
}
