using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Asm.BankPlus.Data.Entities.Ing;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.Extensions.Logging;

namespace Asm.BankPlus.Services.Importers
{
    public class IngImporter : ImporterBase
    {
        private const int Columns = 5;
        private const int DateColumn = 0;
        private const int DescriptionColumn = 1;
        private const int CreditColumn = 2;
        private const int DebitColumn = 3;
        private const int BalanceColumn = 4;

        private static readonly Regex VisaPurchase = new Regex(@"(.*) - Visa Purchase - Receipt \d{6}In (.*) Date (.*) Card (462263xxxxxx\d{4})");
        private static readonly Regex DirectDebit = new Regex(@"(.*) - Direct Debit - Receipt \d{6} (.*)");
        private static readonly Regex DirectPayment = new Regex(@"");


        public IngImporter(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ITransactionTagRuleRepository transactionTagRuleRepository, ILogger<IngImporter> logger) : base(transactionRepository, accountRepository, transactionTagRuleRepository, logger)
        {
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

                string[] columns = line.Split(",");

                #region Validation
                if (columns.Length != Columns)
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

                TransactionExtra extraInfo = ParseDescription(columns[DescriptionColumn]);

                /*if (extraInfo != null)
                {
                    extraInfo.Transaction = transaction;
                }*/

                transactions.Add(transaction);
            }

            await AccountRepository.SetBalance(account.Id, endBalance.Value);

            var savedTransactions = await TransactionRepository.CreateTransactions(transactions);

            return new TransactionImportResult(savedTransactions);
        }

        private TransactionExtra ParseDescription(string description)
        {
            return null;
        }
    }
}
