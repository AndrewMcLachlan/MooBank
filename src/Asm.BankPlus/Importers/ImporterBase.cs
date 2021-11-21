using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Asm.BankPlus.Importers
{
    public abstract class ImporterBase : IImporter
    {
        private readonly ITransactionTagRuleRepository _transactionTagRuleRepository;

        protected ITransactionRepository TransactionRepository { get; }
        protected IAccountRepository AccountRepository { get; }

        protected ILogger<ImporterBase> Logger { get; }

        protected ImporterBase(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ITransactionTagRuleRepository transactionTagRuleRepository, ILogger<ImporterBase> logger)
        {
            TransactionRepository = transactionRepository;
            Logger = logger;
            AccountRepository = accountRepository;
            _transactionTagRuleRepository = transactionTagRuleRepository;
        }

        public async Task<TransactionImportResult> Import(Account account, Stream contents)
        {
            var importResult = await DoImport(account, contents);

            await ApplyTransactionRules(account, importResult);

            return importResult;
        }

        protected abstract Task<TransactionImportResult> DoImport(Account account, Stream contents);

        private async Task ApplyTransactionRules(Account account, TransactionImportResult importResult)
        {
            var rules = await _transactionTagRuleRepository.Get(account.Id);

            foreach(var transaction in importResult.Transactions)
            {
                var applicableTags = rules.Where(r => transaction.Description.Contains(r.Contains, StringComparison.OrdinalIgnoreCase)).SelectMany(r => r.Tags.Select(t => t.Id)).Distinct();

                await TransactionRepository.AddTransactionTags(transaction.Id, applicableTags);
            }

            await TransactionRepository.SaveChanges();
        }
    }
}
