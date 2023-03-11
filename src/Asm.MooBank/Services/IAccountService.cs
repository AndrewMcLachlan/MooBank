using System;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services
{
    public interface IAccountService
    {
        Task<AccountsList> GetFormattedAccounts(CancellationToken cancellation = default);

        void RunTransactionRules(Guid accountId);
    }
}
