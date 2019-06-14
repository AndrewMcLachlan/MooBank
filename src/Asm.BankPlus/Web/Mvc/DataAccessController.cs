using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Mvc
{
    public abstract class BankPlusController : Controller
    {
        protected BankPlusContext DataContext { get; }

        public BankPlusController(BankPlusContext dataContext)
        {
            DataContext = dataContext;
        }
    }
}
