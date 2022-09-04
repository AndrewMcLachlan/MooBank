using System;
using System.Collections.Generic;
using System.Text;
using Asm.MooBank.Data.Entities;

namespace Asm.MooBank.Data.Repositories
{
    public interface ISecurityRepository
    {
        void AssertPermission(Guid accountId);
        void AssertPermission(Account account);
    }
}
