using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;

namespace Asm.BankPlus.Services
{
        public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionTagRuleRepository _transactionTagRuleRepository;
        private readonly ITransactionRepository _transactionRepository;

        public AccountService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ITransactionTagRuleRepository transactionTagRuleRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _transactionTagRuleRepository = transactionTagRuleRepository;
        }

        public async Task<IEnumerable<Transaction>> RunTransactionRules(Guid accountId)
        {
            var transactions = await _transactionRepository.GetTransactions(accountId);

            var rules = await _transactionTagRuleRepository.Get(accountId);

            var updatedTransactions = new List<Transaction>();

            foreach (var transaction in transactions)
            {
                var applicableTags = rules.Where(r => transaction.Description.Contains(r.Contains, StringComparison.OrdinalIgnoreCase)).SelectMany(r => r.Tags.Select(t => t.Id)).Distinct();

                updatedTransactions.Add(await _transactionRepository.AddTransactionTags(transaction.Id, applicableTags));
            }

            await _transactionRepository.SaveChanges();

            return updatedTransactions;
        }

    }
}
