using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Web.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            if (ModelState.IsValid && Authenticate(model))
            {
                return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private bool Authenticate(LoginModel model)
        {
#if DEBUG
            bool result = model.UserName == "andy@andrewmclachlan.com" && model.Password.Length == 8;
#else
            bool result = Membership.ValidateUser(model.UserName, model.Password);
#endif
            if (result)
            {
                FormsAuthentication.SetAuthCookie(model.UserName, false);
            }
            else
            {
                FormsAuthentication.SignOut();
            }

            return result;
        }
	}
}