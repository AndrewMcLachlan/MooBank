using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface IAccountHolderRepository
    {
        Task<AccountHolder> GetCurrent();
    }
}
