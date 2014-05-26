using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Asm.BankPlus.DataAccess;

namespace Asm.BankPlus.Web.Mvc
{
    public class DataAccessController : Controller
    {
        protected readonly BankPlusContext BankPlusDb = new BankPlusContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BankPlusDb.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
