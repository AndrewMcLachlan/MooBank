using System;

namespace Asm.MooBank.Services
{
    public interface IAccountService
    {
        void RunTransactionRules(Guid accountId);
    }
}
