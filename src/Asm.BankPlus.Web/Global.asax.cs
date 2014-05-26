using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Asm.BankPlus.Security;

namespace Asm.BankPlus.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Application.Add("Version", FileVersionInfo.GetVersionInfo(typeof(MvcApplication).Assembly.Location).FileVersion);

#if ENCRYPT
            SecureConfiguration.EncryptConfig("appSettings");
            SecureConfiguration.EncryptConfig("connectionStrings");
#endif
        }
    }
}
