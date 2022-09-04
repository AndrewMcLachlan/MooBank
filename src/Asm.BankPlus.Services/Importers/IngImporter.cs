using System.Globalization;
using System.Text.RegularExpressions;
using Asm.MooBank.Data.Repositories.Ing;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Asm.MooBank.Models.Ing;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services.Importers
{
    public class IngImporter : ImporterBase
    {
        private const int Columns = 5;
        private const int DateColumn = 0;
        private const int DescriptionColumn = 1;
        private const int CreditColumn = 2;
        private const int DebitColumn = 3;
        private const int BalanceColumn = 4;

        private static readonly Regex VisaPurchase = new(@"^(.+) - Visa Purchase - Receipt (\d{6})In (.+) Date (.+) Card (462263xxxxxx\d{4})");
        private static readonly Regex DirectDebit = new(@"^(.+) - Direct Debit - Receipt (\d{6}) (.+)");
        private static readonly Regex InternalTransfer = new(@"^(.+) - Internal Transfer - Receipt (\d{6}) (.*)");
        private static readonly Regex EftposPurchase = new(@"^(.+) - EFTPOS Purchase - Receipt (\d{6})Date (.+) Card (462263xxxxxx\d{4})");
        private static readonly Regex DirectPayment = new(@"^([^-]+) - Receipt (\d{6})");

        private readonly ITransactionExtraRepository _transactionExtraRepository;

        public IngImporter(ITransactionRepository transactionRepository,
                           IAccountRepository accountRepository,
                           ITransactionTagRuleRepository transactionTagRuleRepository,
                           ILogger<IngImporter> logger,
                           ITransactionExtraRepository transactionExtraRepository) : base(transactionRepository, accountRepository, transactionTagRuleRepository, logger)
        {
            _transactionExtraRepository = transactionExtraRepository;
        }

        protected override async Task<TransactionImportResult> DoImport(Account account, Stream contents)
        {
            using var reader = new StreamReader(contents);
            var transactions = new List<Transaction>();

            // Throw away header row
            await reader.ReadLineAsync();

            int lineCount = 2;

            decimal? endBalance = null;

            while (!reader.EndOfStream)
            {
                DateTime transactionTime = DateTime.MinValue;
                decimal credit = 0;
                decimal debit = 0;

                string line = await reader.ReadLineAsync();

                string[] prelimColumns = line.Split(",");

                List<string> columns = new();

                string current = null;

                foreach(string str in prelimColumns)
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
                    Logger.LogWarning($"Unrecognised entry at line {lineCount}");
                    continue;
                }

                if (!DateTime.TryParseExact(columns[DateColumn], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out transactionTime))
                {
                    Logger.LogWarning($"Incorrect date format at line {lineCount}");
                    continue;
                }
                if (String.IsNullOrWhiteSpace(columns[DescriptionColumn]))
                {
                    Logger.LogWarning($"Description not supplied at line {lineCount}");
                    continue;
                }

                if (String.IsNullOrEmpty(columns[CreditColumn]) && String.IsNullOrEmpty(columns[DebitColumn]) || !String.IsNullOrEmpty(columns[CreditColumn]) && !String.IsNullOrEmpty(columns[DebitColumn]))
                {
                    Logger.LogWarning($"Credit or Debit amount not supplied at line {lineCount}");
                    continue;
                }

                if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], out credit))
                {
                    Logger.LogWarning($"Incorrect credit format at line {lineCount}");
                    continue;
                }
                else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], out debit))
                {
                    Logger.LogWarning($"Incorrect debit format at line {lineCount}");
                    continue;
                }

                TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

                if (!Decimal.TryParse(columns[BalanceColumn], out decimal balance))
                {
                    Logger.LogWarning($"Incorrect balance format at line {lineCount}");
                    continue;
                }
                #endregion

                if (endBalance == null) endBalance = balance;

                var transaction = new Transaction
                {
                    AccountId = account.Id,
                    Amount = transactionType == TransactionType.Credit ? credit : debit,
                    Description = columns[DescriptionColumn],
                    TransactionTime = transactionTime,
                    TransactionType = transactionType,
                };

                transactions.Add(transaction);
            }

            await AccountRepository.SetBalance(account.Id, endBalance.Value);

            var savedTransactions = await TransactionRepository.CreateTransactions(transactions);

            var transactionExtras = new List<TransactionExtra>();

            foreach(var transaction in savedTransactions)
            {
                TransactionExtra extraInfo = ParseDescription(transaction);

                if (extraInfo != null)
                {
                    transactionExtras.Add(extraInfo);
                }

            }

            await _transactionExtraRepository.CreateTransactionExtras(transactionExtras);

            return new TransactionImportResult(savedTransactions);
        }

        private static TransactionExtra ParseDescription(Transaction transaction)
        {
            var match = VisaPurchase.Match(transaction.Description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    TransactionId = transaction.Id,
                    Description = match.Groups[1].Value,
                    Location = match.Groups[3].Value,
                    PurchaseDate = DateTime.Parse(match.Groups[4].Value),
                    PurchaseType = "Visa",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                };
            }

            match = DirectDebit.Match(transaction.Description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    TransactionId = transaction.Id,
                    Description = match.Groups[1].Value,
                    PurchaseType = "Direct Debit",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    Reference = match.Groups[3].Value,
                };
            }

            match = InternalTransfer.Match(transaction.Description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    TransactionId = transaction.Id,
                    Description = match.Groups[1].Value,
                    PurchaseType = "Internal Transfer",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    Reference = match.Groups[3].Value
                };
            }

            match = EftposPurchase.Match(transaction.Description);

            if (match.Success)
            {
                return new TransactionExtra
                {
                    TransactionId = transaction.Id,
                    Description = match.Groups[1].Value,
                    PurchaseDate = DateTime.ParseExact(match.Groups[3].Value, "dd MMM yyyy Ti\\me h:mmtt", CultureInfo.CurrentCulture),
                    PurchaseType = "Visa",
                    ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                };
            }

            match = DirectPayment.Match(transaction.Description);
            {
                if (match.Success)
                {
                    return new TransactionExtra
                    {
                        TransactionId = transaction.Id,
                        Description = match.Groups[1].Value,
                        PurchaseType = "Direct",
                        ReceiptNumber = Int32.Parse(match.Groups[2].Value),
                    };
                }
            }

            return null;
        }
    }
}
