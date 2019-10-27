using System;
using System.Collections.Generic;
using System.Text;
using Asm.BankPlus.Data;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Repository.Ing;

namespace Asm.BankPlus.Services.Ing
{
    public class TransactionExtraRepository : DataRepository, ITransactionExtraRepository
    {
        public TransactionExtraRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }
    }
}
