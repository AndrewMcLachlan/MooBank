using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Asm.BankPlus.Web.Models
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel
    {
        public int? HttpErrorCode { get; set; }
        public string HttpError { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
    }
}