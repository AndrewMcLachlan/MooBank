using System;
using System.Collections.Generic;
using System.Text;
using Asm.BankPlus.Data.Entities;

namespace Asm.BankPlus.Repository
{
    public interface ISecurityRepository
    {
        void AssertPermission(Guid accountId);
        void AssertPermission(Account account);
    }
}
