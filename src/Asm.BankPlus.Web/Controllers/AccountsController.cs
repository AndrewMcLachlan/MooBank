using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asm.BankPlus.Web.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Authorize]
    public class AccountsController : DataAccessController
    {
        [AcceptVerbs(HttpVerbs.Patch)]
        public ActionResult UpdateBalance(Guid id, decimal currentBalance, decimal availableBalance)
        {
            var account = BankPlusDb.Accounts.Where(a => a.AccountId == id).SingleOrDefault();

            if (account == null)
            {
                return new HttpStatusCodeResult(404, "Not Found");
            }

            account.AccountBalance = currentBalance;
            account.AvailableBalance = availableBalance;

            BankPlusDb.SaveChanges();

            return new HttpStatusCodeResult(200);
        }
	}
}