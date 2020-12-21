using System;

namespace Asm.BankPlus.Services
{
    public interface IAccountService
    {
        void RunTransactionRules(Guid accountId);
    }
}
