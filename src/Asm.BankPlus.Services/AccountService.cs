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
        private readonly IRunRulesQueue _queue;

        public AccountService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ITransactionTagRuleRepository transactionTagRuleRepository, IRunRulesQueue queue)
        {
            _queue = queue;
        }

        public void RunTransactionRules(Guid accountId)
        {
            _queue.QueueRunRules(accountId);
        }
    }
}
