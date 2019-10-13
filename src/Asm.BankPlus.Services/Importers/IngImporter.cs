using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Importers;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.Extensions.Logging;

namespace Asm.BankPlus.Services.Importers
{
    public class IngImporter : IImporter
    {
        private const int Columns = 5;
        private const int DateColumn = 0;
        private const int DescriptionColumn = 1;
        private const int CreditColumn = 2;
        private const int DebitColumn = 3;
        private const int BalanceColumn = 4;

        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<IngImporter> _logger;


        public IngImporter(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ILogger<IngImporter> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
            _accountRepository = accountRepository;
        }

        public async Task Import(Account account, Stream contents)
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
                    _logger.LogWarning($"Unrecognised entry at line {lineCount}");
                    continue;
                }

                if (!DateTime.TryParseExact(columns[DateColumn], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out transactionTime))
                {
                    _logger.LogWarning($"Incorrect date format at line {lineCount}");
                    continue;
                }
                if (String.IsNullOrWhiteSpace(columns[DescriptionColumn]))
                {
                    _logger.LogWarning($"Description not supplied at line {lineCount}");
                    continue;
                }

                if (String.IsNullOrEmpty(columns[CreditColumn]) && String.IsNullOrEmpty(columns[DebitColumn]) || !String.IsNullOrEmpty(columns[CreditColumn]) && !String.IsNullOrEmpty(columns[DebitColumn]))
                {
                    _logger.LogWarning($"Credit or Debit amount not supplied at line {lineCount}");
                    continue;
                }

                if (!String.IsNullOrEmpty(columns[CreditColumn]) && !Decimal.TryParse(columns[CreditColumn], out credit))
                {
                    _logger.LogWarning($"Incorrect credit format at line {lineCount}");
                    continue;
                }
                else if (!String.IsNullOrEmpty(columns[DebitColumn]) && !Decimal.TryParse(columns[DebitColumn], out debit))
                {
                    _logger.LogWarning($"Incorrect debit format at line {lineCount}");
                    continue;
                }

                TransactionType transactionType = !String.IsNullOrEmpty(columns[CreditColumn]) ? TransactionType.Credit : TransactionType.Debit;

                if (!Decimal.TryParse(columns[BalanceColumn], out decimal balance))
                {
                    _logger.LogWarning($"Incorrect balance format at line {lineCount}");
                    continue;
                }
                #endregion

                if (endBalance == null) endBalance = balance;

                transactions.Add(new Transaction
                {
                    AccountId = account.Id,
                    Amount = transactionType == TransactionType.Credit ? credit : debit,
                    Description = columns[DescriptionColumn],
                    TransactionTime = transactionTime,
                    TransactionType = transactionType,
                });
            }

            await _accountRepository.SetBalance(account.Id, endBalance.Value);

            await _transactionRepository.CreateTransactions(transactions);

        }
    }
}
