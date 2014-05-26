using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Asm.BankPlus.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static string AppName(this HtmlHelper html)
        {
            return ConfigurationManager.AppSettings["AppName"];
        }

        public static string Skin(this HtmlHelper html)
        {
            return ConfigurationManager.AppSettings["Skin"].ToLowerInvariant();
        }
    }
}
