using System;
using System.Collections.Generic;
using System.Text;
using Asm.BankPlus.Data;

namespace Asm.BankPlus.Repository
{
    public abstract class DataRepository
    {
        protected BankPlusContext DataContext { get; }

        protected DataRepository(BankPlusContext dataContext)
        {
            DataContext = dataContext;
        }
    }
}
