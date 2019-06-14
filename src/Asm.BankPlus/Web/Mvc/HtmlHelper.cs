using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;

namespace Asm.BankPlus.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static string AppName(this IHtmlHelper<dynamic> html)
        {
            return ((IConfiguration)html.ViewContext.HttpContext.RequestServices.GetService(typeof(IConfiguration))).GetValue<string>("AppName");
        }

        public static string Skin(this IHtmlHelper<dynamic> html)
        {
            return ((IConfiguration)html.ViewContext.HttpContext.RequestServices.GetService(typeof(IConfiguration))).GetValue<string>("Skin");
        }
    }
}
