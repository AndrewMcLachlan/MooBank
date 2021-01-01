using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data.Entities;

namespace Asm.BankPlus.Repository
{
    public interface IAccountHolderRepository
    {
        Task<AccountHolder> GetCurrent();
    }
}
