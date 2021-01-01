using System;

namespace Asm.BankPlus.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRunRulesQueue _queue;

        public AccountService(IRunRulesQueue queue)
        {
            _queue = queue;
        }

        public void RunTransactionRules(Guid accountId)
        {
            _queue.QueueRunRules(accountId);
        }
    }
}
