using Asm.MooBank.Models;

namespace Asm.MooBank.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRunRulesQueue _queue;
        private readonly IAccountRepository _accountRepository;

        public AccountService(IRunRulesQueue queue, IAccountRepository accountRepository)
        {
            _queue = queue;
            _accountRepository = accountRepository;
        }

        public async Task<AccountsList> GetFormattedAccounts(CancellationToken cancellation = default)
        {
            var accounts = await _accountRepository.GetAccounts();
            var position = await _accountRepository.GetPosition();

            return new AccountsList
            {

                PositionedAccounts = accounts.Where(a => a.IncludeInPosition),
                OtherAccounts = accounts.Where(a => !a.IncludeInPosition),
                Position = position,
            };
        }

        public void RunTransactionRules(Guid accountId)
        {
            _queue.QueueRunRules(accountId);
        }
    }
}
